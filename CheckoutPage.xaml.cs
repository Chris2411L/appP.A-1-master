using appP.A.Models;
using appP.A.Services;
using Microsoft.Maui.Storage;
using System.Globalization;
using System.Text.Json;

namespace appP.A
{
    public partial class CheckoutPage : ContentPage
    {
        private double _total;
        private static readonly HttpClient client = new HttpClient();
        private List<string> _nombresDir = new();

        private double _selectedLat;
        private double _selectedLon;
        private bool _manualLocationSelected;
        private string _currentDireccion = "";

        public CheckoutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Calcular();
            CargarDirecciones();

            DeliveryDateLabel.Text = DateTime.Now.AddDays(3)
                .ToString("dddd, dd 'de' MMMM", new CultureInfo("es-MX"))
                .ToUpper();

            CargarMapaInicial();

            CheckoutContent.Opacity = 0;
            CheckoutContent.TranslationY = 18;
            PayButton.Opacity = 0;

            await Task.WhenAll(
                CheckoutContent.FadeTo(1, 350, Easing.CubicOut),
                CheckoutContent.TranslateTo(0, 0, 350, Easing.CubicOut),
                PayButton.FadeTo(1, 450, Easing.CubicOut)
            );
        }

        private void CargarDirecciones()
        {
            var user = AuthService.GetCurrentUser() ?? "invitado";
            string lista = Preferences.Get($"lista_direcciones_{user}", "");

            if (!string.IsNullOrEmpty(lista))
            {
                _nombresDir = lista.Split('|').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                SavedAddressesPicker.ItemsSource = _nombresDir;

                if (_nombresDir.Count > 0)
                    SavedAddressesPicker.SelectedIndex = 0;
            }
        }

        private void OnSavedAddressChanged(object sender, EventArgs e)
        {
            if (SavedAddressesPicker.SelectedIndex == -1) return;

            var user = AuthService.GetCurrentUser() ?? "invitado";
            string key = SavedAddressesPicker.SelectedItem?.ToString() ?? "";

            string calle = Preferences.Get($"{user}_{key}_calle", "");
            string ciudad = Preferences.Get($"{user}_{key}_ciu", "");
            string col = Preferences.Get($"{user}_{key}_col", "");
            string num = Preferences.Get($"{user}_{key}_num", "");
            string cp = Preferences.Get($"{user}_{key}_cp", "");

            _currentDireccion = $"{calle} {num}, {col}, {cp}, {ciudad}, México";
            DireccionSeleccionadaLabel.Text = $"{key}: {_currentDireccion}";

            _manualLocationSelected = false;
            MapaPrecisionLabel.Text = "Buscando ubicación exacta por calle, número, colonia, C.P. y ciudad...";

            ActualizarMapa(_currentDireccion);
        }

        private async void ActualizarMapa(string direccionCompleta)
        {
            try
            {
                string q = Uri.EscapeDataString(direccionCompleta);

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "NontonioApp");

                var res = await client.GetStringAsync(
                    $"https://nominatim.openstreetmap.org/search?format=json&q={q}&addressdetails=1&limit=1");

                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement.EnumerateArray().FirstOrDefault();

                if (root.ValueKind != JsonValueKind.Undefined)
                {
                    string? lat = root.GetProperty("lat").GetString();
                    string? lon = root.GetProperty("lon").GetString();

                    if (double.TryParse(lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lt) &&
                        double.TryParse(lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double ln))
                    {
                        _selectedLat = lt;
                        _selectedLon = ln;

                        MapaPrecisionLabel.Text = "Ubicación encontrada. Si no cae exacta, toca tu casa en el mapa.";
                        CargarMapaInteractivo(_selectedLat, _selectedLon);
                        return;
                    }
                }

                CargarMapaInicial();
                MapaPrecisionLabel.Text = "No se detectó exacto. Toca el mapa para marcar tu casa.";
            }
            catch
            {
                CargarMapaInicial();
                MapaPrecisionLabel.Text = "No se pudo detectar. Toca el mapa para marcar tu casa.";
            }
        }

        private void CargarMapaInicial()
        {
            _selectedLat = 19.4326;
            _selectedLon = -99.1332;
            CargarMapaInteractivo(_selectedLat, _selectedLon);
        }

        private void CargarMapaInteractivo(double lat, double lon)
        {
            string latTxt = lat.ToString(CultureInfo.InvariantCulture);
            string lonTxt = lon.ToString(CultureInfo.InvariantCulture);

            string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<style>
html, body, #map {{
    height:100%;
    margin:0;
    padding:0;
    background:#111;
}}
.leaflet-control-attribution {{ display:none; }}
</style>
</head>
<body>
<div id='map'></div>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<script>
var map = L.map('map').setView([{latTxt}, {lonTxt}], 18);

L.tileLayer('https://{{s}}.basemaps.cartocdn.com/light_all/{{z}}/{{x}}/{{y}}{{r}}.png', {{
    maxZoom: 19,
    subdomains: 'abcd'
}}).addTo(map);

var iconCasa = L.divIcon({{
    html: '<div style=""font-size:32px;background:white;border:3px solid black;border-radius:50%;width:44px;height:44px;display:flex;align-items:center;justify-content:center;"">🏠</div>',
    className: '',
    iconSize: [44,44],
    iconAnchor: [22,22]
}});

var marker = L.marker([{latTxt}, {lonTxt}], {{ icon: iconCasa, draggable:true }}).addTo(map);

function send(lat, lon) {{
    window.location.href = 'nontonio://location?lat=' + lat + '&lon=' + lon;
}}

map.on('click', function(e) {{
    marker.setLatLng(e.latlng);
    send(e.latlng.lat, e.latlng.lng);
}});

marker.on('dragend', function(e) {{
    var p = marker.getLatLng();
    send(p.lat, p.lng);
}});
</script>
</body>
</html>";

            MapView.Source = new HtmlWebViewSource { Html = html };
        }

        private void MapView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("nontonio://location", StringComparison.OrdinalIgnoreCase))
                return;

            e.Cancel = true;

            try
            {
                var uri = new Uri(e.Url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                string? latText = query.Get("lat");
                string? lonText = query.Get("lon");

                if (double.TryParse(latText, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonText, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                {
                    _selectedLat = lat;
                    _selectedLon = lon;
                    _manualLocationSelected = true;

                    MapaPrecisionLabel.Text = $"Casa marcada manualmente: {lat:F5}, {lon:F5}";
                }
            }
            catch { }
        }

        private void OnCentrarMapaClicked(object sender, EventArgs e)
        {
            CargarMapaInteractivo(_selectedLat, _selectedLon);
        }

        private void Calcular()
        {
            double sub = AppData.CarritoActual.Total;
            double iva = sub * 0.16;
            double tar = 15.00;
            double env = sub >= 299 ? 0 : 99.00;

            _total = sub + iva + tar + env;

            SubtotalLabel.Text = sub.ToString("C", CultureInfo.CurrentCulture);
            IvaLabel.Text = iva.ToString("C", CultureInfo.CurrentCulture);
            TarifaLabel.Text = tar.ToString("C", CultureInfo.CurrentCulture);
            ShippingLabel.Text = env == 0 ? "GRATIS" : env.ToString("C", CultureInfo.CurrentCulture);
            TotalLabel.Text = _total.ToString("C", CultureInfo.CurrentCulture);
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

            if (!_manualLocationSelected)
            {
                bool continuar = await DisplayAlert(
                    "Ubicación",
                    "La ubicación fue tomada automáticamente. Para máxima precisión, toca el mapa justo sobre tu casa.\n\n¿Deseas continuar así?",
                    "Continuar",
                    "Corregir");

                if (!continuar)
                    return;
            }

            var orden = new Orden
            {
                Usuario = AuthService.GetCurrentUser() ?? "invitado",
                Direccion = _currentDireccion,
                Total = _total,
                Fecha = DateTime.Now,
                MetodoPago = PaymentPicker.SelectedItem?.ToString() ?? "",
                Detalles = string.Join(", ", AppData.CarritoActual.Productos.Select(p => $"{p.Cantidad}x {p.Nombre}")),
                DestLat = _selectedLat,
                DestLon = _selectedLon
            };

            var sucursal = SucursalesService.ObtenerMasCercana(orden.DestLat, orden.DestLon);

            orden.BranchLat = sucursal.Lat;
            orden.BranchLon = sucursal.Lon;
            orden.BranchName = sucursal.Nombre;

            orden.CurrentLat = sucursal.Lat + 0.003;
            orden.CurrentLon = sucursal.Lon - 0.003;
            orden.Status = "Preparando";

            orden.HistoryJson = JsonSerializer.Serialize(new List<string>
            {
                $"{DateTime.Now:g}: Pedido creado en {sucursal.Nombre}"
            });

            double degToKm = 111;
            double dx = (sucursal.Lat - orden.DestLat) * degToKm;
            double dy = (sucursal.Lon - orden.DestLon) * degToKm * Math.Cos(orden.DestLat * Math.PI / 180);
            double distKm = Math.Sqrt(dx * dx + dy * dy);

            int etaMinutes = 20 + (int)(distKm * 6);
            orden.EstimatedDelivery = DateTime.Now.AddMinutes(etaMinutes);

            await OrdenesService.GuardarOrdenAsync(orden);

            foreach (var p in AppData.CarritoActual.Productos.ToList())
            {
                var prod = AppData.Categorias
                    .SelectMany(c => c.Productos)
                    .FirstOrDefault(x => x.Id == p.Id || x.Nombre == p.Nombre);

                if (prod != null)
                {
                    prod.Stock = Math.Max(0, prod.Stock - p.Cantidad);

                    try { await InventarioService.ActualizarStockAsync(prod.Id, prod.Stock); } catch { }
                    try { await ProductService.UpdateAsync(prod); } catch { }
                }
            }

            AppData.CarritoActual.Vaciar();

            await DisplayAlert("Éxito", "¡Tu pedido está en camino!", "Aceptar");
            await Navigation.PopToRootAsync();
        }
    }
}