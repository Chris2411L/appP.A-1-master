using appP.A.Models;
using appP.A.Services;
using System.Globalization;
using System.Text.Json;

namespace appP.A
{
    public partial class CheckoutPage : ContentPage
    {
        private double _total;
        private static readonly HttpClient client = new HttpClient();
        private List<string> _nombresDir = new List<string>();

        public CheckoutPage() { InitializeComponent(); }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Calcular();
            CargarDirecciones();
            DeliveryDateLabel.Text = DateTime.Now.AddDays(3).ToString("dddd, dd de MMMM").ToUpper();
        }

        private void CargarDirecciones()
        {
            var user = AuthService.GetCurrentUser() ?? "invitado";
            string lista = Preferences.Get($"lista_direcciones_{user}", "");
            if (!string.IsNullOrEmpty(lista))
            {
                _nombresDir = lista.Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
                SavedAddressesPicker.ItemsSource = _nombresDir;
                if (_nombresDir.Count > 0) SavedAddressesPicker.SelectedIndex = 0;
            }
        }

        private void OnSavedAddressChanged(object sender, EventArgs e)
        {
            if (SavedAddressesPicker.SelectedIndex == -1) return;

            var user = AuthService.GetCurrentUser() ?? "invitado";
            string key = SavedAddressesPicker.SelectedItem.ToString();

            string calle = Preferences.Get($"{user}_{key}_calle", "");
            string ciudad = Preferences.Get($"{user}_{key}_ciu", "");
            string col = Preferences.Get($"{user}_{key}_col", "");
            string num = Preferences.Get($"{user}_{key}_num", "");

            DireccionSeleccionadaLabel.Text = $"{key}: {calle} {num}, {ciudad}";

            // Actualizar mapa (solo lectura)
            ActualizarMapa(calle, num, col, ciudad);
        }

        private async void ActualizarMapa(string calle, string num, string col, string ciudad)
        {
            try
            {
                string q = Uri.EscapeDataString($"{calle} {num}, {col}, {ciudad}, Mexico");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "NontonioApp");
                var res = await client.GetStringAsync($"https://nominatim.openstreetmap.org/search?format=json&q={q}&limit=1");
                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement.EnumerateArray().FirstOrDefault();
                if (root.ValueKind != JsonValueKind.Undefined)
                {
                    string lat = root.GetProperty("lat").GetString();
                    string lon = root.GetProperty("lon").GetString();
                    double lt = double.Parse(lat, CultureInfo.InvariantCulture);
                    double ln = double.Parse(lon, CultureInfo.InvariantCulture);
                    string bbox = $"{(ln - 0.002).ToString(CultureInfo.InvariantCulture)},{(lt - 0.002).ToString(CultureInfo.InvariantCulture)},{(ln + 0.002).ToString(CultureInfo.InvariantCulture)},{(lt + 0.002).ToString(CultureInfo.InvariantCulture)}";
                    MapView.Source = $"https://www.openstreetmap.org/export/embed.html?bbox={bbox}&layer=mapnik&marker={lat},{lon}";
                }
            }
            catch { }
        }

        private void Calcular()
        {
            double sub = AppData.CarritoActual.Total;
            double iva = sub * 0.16;
            double tar = 15.00;
            double env = sub >= 299 ? 0 : 99.00;
            _total = sub + iva + tar + env;

            SubtotalLabel.Text = sub.ToString("C");
            IvaLabel.Text = iva.ToString("C");
            TarifaLabel.Text = tar.ToString("C");
            ShippingLabel.Text = env == 0 ? "GRATIS" : env.ToString("C");
            TotalLabel.Text = _total.ToString("C");
        }

        private async void OnGoToProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MisComprasPage());
        }

        private async void OnConfirmOrderClicked(object sender, EventArgs e)
        {
            if (SavedAddressesPicker.SelectedIndex == -1 || PaymentPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Atención", "Selecciona una dirección y un método de pago.", "OK");
                return;
            }

            var orden = new Orden
            {
                Usuario = AuthService.GetCurrentUser() ?? "invitado",
                Direccion = DireccionSeleccionadaLabel.Text,
                Total = _total,
                Fecha = DateTime.Now,
                MetodoPago = PaymentPicker.SelectedItem.ToString(),
                Detalles = string.Join(", ", AppData.CarritoActual.Productos.Select(p => $"{p.Cantidad}x {p.Nombre}"))
            };

            // Intentar geocodificar la dirección para seguimiento (OpenStreetMap Nominatim)
            try
            {
                string q = Uri.EscapeDataString(orden.Direccion + ", Mexico");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "NontonioApp");
                var res = await client.GetStringAsync($"https://nominatim.openstreetmap.org/search?format=json&q={q}&limit=1");
                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement.EnumerateArray().FirstOrDefault();
                if (root.ValueKind != JsonValueKind.Undefined)
                {
                    string lat = root.GetProperty("lat").GetString();
                    string lon = root.GetProperty("lon").GetString();
                    orden.DestLat = double.Parse(lat, CultureInfo.InvariantCulture);
                    orden.DestLon = double.Parse(lon, CultureInfo.InvariantCulture);
                }
            }
            catch { }

            // Inicializar tracking: poner repartidor en una posición cercana (simulada)
            orden.CurrentLat = orden.DestLat + 0.01; // ~1km al norte
            orden.CurrentLon = orden.DestLon - 0.01; // ~1km al oeste
            orden.Status = "Preparando";
            orden.HistoryJson = System.Text.Json.JsonSerializer.Serialize(new List<string> { $"{DateTime.Now:g}: Pedido creado" });
            orden.EstimatedDelivery = DateTime.Now.AddMinutes(30);

            await OrdenesService.GuardarOrdenAsync(orden);
            await DisplayAlert("Éxito", "¡Tu pedido está en camino!", "Aceptar");
            // Reducir stock y persistir en inventario/product service
            foreach (var p in AppData.CarritoActual.Productos.ToList())
            {
                // Buscar producto en AppData por Id o nombre
                var prod = AppData.Categorias.SelectMany(c => c.Productos).FirstOrDefault(x => x.Id == p.Id || x.Nombre == p.Nombre);
                if (prod != null)
                {
                    prod.Stock = Math.Max(0, prod.Stock - p.Cantidad);
                    // Actualizar en InventarioService y ProductService
                    try { await InventarioService.ActualizarStockAsync(prod.Id, prod.Stock); } catch { }
                    try { await Services.ProductService.UpdateAsync(prod); } catch { }
                }
            }

            AppData.CarritoActual.Vaciar();
            await Navigation.PopToRootAsync();
        }
    }
}