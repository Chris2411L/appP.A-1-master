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
                    MapView.Source = new UrlWebViewSource { Url = $"https://www.openstreetmap.org/export/embed.html?bbox={bbox}&layer=mapnik&marker={lat},{lon}" };
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
            // Choose nearest branch (sucursal) and set courier starting point relative to branch
            // Branches across Mexico (approximate coordinates). The nearest branch will be selected.
            var branches = new List<(double Lat, double Lon, string Name)>
            {
                (19.432608, -99.133209, "Sucursal CDMX - Centro"),
                (20.659698, -103.349609, "Sucursal Guadalajara"),
                (25.686614, -100.316113, "Sucursal Monterrey"),
                (19.041297, -98.206200, "Sucursal Puebla"),
                (32.514946, -117.038247, "Sucursal Tijuana"),
                (20.967370, -89.592586, "Sucursal Mérida"),
                (21.161908, -86.851528, "Sucursal Cancún"),
                (21.122219, -101.677392, "Sucursal León"),
                (20.588793, -100.389888, "Sucursal Querétaro"),
                (19.292046, -99.653942, "Sucursal Toluca"),
                (17.073184, -96.726585, "Sucursal Oaxaca"),
                (19.173773, -96.134224, "Sucursal Veracruz"),
                (28.633891, -106.069100, "Sucursal Chihuahua"),
                (25.548383, -103.411782, "Sucursal Torreón"),
                (21.882344, -102.282593, "Sucursal Aguascalientes"),
                (22.156469, -100.985540, "Sucursal San Luis P."),
                (19.703631, -101.184884, "Sucursal Morelia"),
                (29.072967, -110.955919, "Sucursal Hermosillo"),
                (24.809064, -107.394014, "Sucursal Culiacán"),
                (23.249391, -106.411140, "Sucursal Mazatlán")
            };

            // Haversine distance to find nearest branch
            static double Haversine(double lat1, double lon1, double lat2, double lon2)
            {
                double R = 6371; // km
                double dLat = (lat2 - lat1) * Math.PI / 180.0;
                double dLon = (lon2 - lon1) * Math.PI / 180.0;
                double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) + Math.Cos(lat1 * Math.PI/180.0) * Math.Cos(lat2 * Math.PI/180.0) * Math.Sin(dLon/2) * Math.Sin(dLon/2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
                return R * c;
            }

            var nearest = branches.OrderBy(b => Haversine(orden.DestLat, orden.DestLon, b.Lat, b.Lon)).First();

            // Store branch info in order
            orden.BranchLat = nearest.Lat;
            orden.BranchLon = nearest.Lon;
            orden.BranchName = nearest.Name;

            // Set courier starting position near the branch (small offset)
            orden.CurrentLat = nearest.Lat + 0.003;
            orden.CurrentLon = nearest.Lon - 0.003;
            orden.Status = "Preparando";
            orden.HistoryJson = System.Text.Json.JsonSerializer.Serialize(new List<string> { $"{DateTime.Now:g}: Pedido creado en {nearest.Name}" });

            // Estimate delivery based on straight-line distance (approximate)
            double degToKm = 111; // rough conversion
            double dx = (nearest.Lat - orden.DestLat) * degToKm;
            double dy = (nearest.Lon - orden.DestLon) * degToKm * Math.Cos(orden.DestLat * Math.PI / 180);
            double distKm = Math.Sqrt(dx * dx + dy * dy);
            int etaMinutes = 20 + (int)(distKm * 6); // base 20min + 6 min per km
            orden.EstimatedDelivery = DateTime.Now.AddMinutes(etaMinutes);

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