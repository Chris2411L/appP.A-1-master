using appP.A.Models;
using appP.A.Services;
using System.Globalization;
using System.Text.Json;

namespace appP.A
{
    public partial class RepartidorPage : ContentPage
    {
        private RepartidorProfile? _perfil;
        private List<Orden> _pedidosActivos = new();
        private Orden? _pedidoSeleccionado;

        private bool _mostrarRuta = false;
        private bool _simulandoEntrega = false;

        private static readonly HttpClient client = new HttpClient();

        public RepartidorPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPerfil();
            await CargarPedidos();
        }

        private async Task CargarPerfil()
        {
            var user = AuthService.GetCurrentUser();

            if (string.IsNullOrWhiteSpace(user))
                return;

            _perfil = await RepartidorService.ObtenerAsync(user);

            if (_perfil == null)
            {
                Application.Current!.Windows[0].Page =
                    new NavigationPage(new RepartidorRegistroPage());
                return;
            }

            NombreLabel.Text = _perfil.Nombre;
            PlacasLabel.Text = $"Placas: {_perfil.Placas}";
            TelefonoLabel.Text = $"Tel: {_perfil.Telefono}";
            SaldoLabel.Text = _perfil.Saldo.ToString("C2");
            ViajesLabel.Text = $"{_perfil.Viajes} viajes";

            if (!string.IsNullOrWhiteSpace(_perfil.FotoPath) && File.Exists(_perfil.FotoPath))
                FotoRepartidor.Source = ImageSource.FromFile(_perfil.FotoPath);
        }

        private async Task CargarPedidos()
        {
            var todas = await OrdenesService.ObtenerTodasOrdenesAsync();

            _pedidosActivos = todas
                .Where(o => o.Status != "Entregado")
                .OrderByDescending(o => o.Fecha)
                .ToList();

            await AsegurarCoordenadasPedidos();
            await CargarMapaPedidosAsync();
        }

        private async Task AsegurarCoordenadasPedidos()
        {
            foreach (var orden in _pedidosActivos)
            {
                if (orden.DestLat != 0 && orden.DestLon != 0)
                    continue;

                var coords = await ObtenerCoordenadasPorDireccion(orden.Direccion);

                if (coords != null)
                {
                    orden.DestLat = coords.Value.lat;
                    orden.DestLon = coords.Value.lon;

                    try
                    {
                        await OrdenesService.ActualizarOrdenAsync(orden);
                    }
                    catch { }
                }
            }
        }

        private async Task<(double lat, double lon)?> ObtenerCoordenadasPorDireccion(string direccion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(direccion))
                    return null;

                string limpia = LimpiarDireccion(direccion);
                string query = Uri.EscapeDataString($"{limpia}, México");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "NontonioApp-Repartidor");

                string url = $"https://nominatim.openstreetmap.org/search?format=json&q={query}&limit=1&addressdetails=1";
                string json = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var item = doc.RootElement.EnumerateArray().FirstOrDefault();

                if (item.ValueKind == JsonValueKind.Undefined)
                    return null;

                string? latText = item.GetProperty("lat").GetString();
                string? lonText = item.GetProperty("lon").GetString();

                if (double.TryParse(latText, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonText, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                {
                    return (lat, lon);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private string LimpiarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                return "";

            string limpia = direccion;

            limpia = limpia.Replace("Casa:", "", StringComparison.OrdinalIgnoreCase);
            limpia = limpia.Replace("Dirección:", "", StringComparison.OrdinalIgnoreCase);
            limpia = limpia.Replace("Cliente:", "", StringComparison.OrdinalIgnoreCase);
            limpia = limpia.Replace(":", " ");

            return limpia.Trim();
        }

        private async Task CargarMapaPedidosAsync()
        {
            double baseLat = 19.4326;
            double baseLon = -99.1332;

            if (_pedidosActivos.Count > 0)
            {
                var primero = _pedidosActivos[0];

                if (primero.DestLat != 0 && primero.DestLon != 0)
                {
                    baseLat = primero.DestLat;
                    baseLon = primero.DestLon;
                }
                else if (primero.BranchLat != 0 && primero.BranchLon != 0)
                {
                    baseLat = primero.BranchLat;
                    baseLon = primero.BranchLon;
                }
            }

            double repartidorLat = baseLat + 0.015;
            double repartidorLon = baseLon - 0.015;

            string markers = "";
            string bounds = $"[{Txt(repartidorLat)}, {Txt(repartidorLon)}]";
            string rutas = "";

            markers += $@"
var iconYo = L.divIcon({{
    html: '<div style=""font-size:34px;background:white;border:3px solid black;border-radius:50%;width:46px;height:46px;display:flex;align-items:center;justify-content:center;"">🏍️</div>',
    className: '',
    iconSize: [46,46],
    iconAnchor: [23,23]
}});

L.marker([{Txt(repartidorLat)}, {Txt(repartidorLon)}], {{icon: iconYo}})
.addTo(map)
.bindPopup('<b>🏍️ Tú</b><br>Repartidor disponible');";

            foreach (var p in _pedidosActivos)
            {
                double casaLat = p.DestLat != 0 ? p.DestLat : baseLat;
                double casaLon = p.DestLon != 0 ? p.DestLon : baseLon;

                double tiendaLat = p.BranchLat != 0 ? p.BranchLat : casaLat - 0.010;
                double tiendaLon = p.BranchLon != 0 ? p.BranchLon : casaLon + 0.010;

                bounds += $", [{Txt(casaLat)}, {Txt(casaLon)}], [{Txt(tiendaLat)}, {Txt(tiendaLon)}]";

                string direccion = JavaSafe(p.Direccion);
                string detalles = JavaSafe(p.Detalles);
                string status = JavaSafe(p.Status);
                string tienda = JavaSafe(string.IsNullOrWhiteSpace(p.BranchName) ? "Tienda NONTONIO" : p.BranchName);

                var ruta = await ObtenerRutaOsrm(repartidorLat, repartidorLon, tiendaLat, tiendaLon, casaLat, casaLon);

                string infoRuta = ruta.ok
                    ? $"Ruta real: {ruta.km:F1} km • {ruta.min} min"
                    : "Ruta aproximada";

                string popupInfoRuta = JavaSafe(infoRuta);

                markers += $@"
var iconCasa{p.Id} = L.divIcon({{
    html: '<div style=""font-size:34px;background:white;border:3px solid black;border-radius:50%;width:46px;height:46px;display:flex;align-items:center;justify-content:center;"">🏠</div>',
    className: '',
    iconSize: [46,46],
    iconAnchor: [23,23]
}});

var iconTienda{p.Id} = L.divIcon({{
    html: '<div style=""font-size:34px;background:white;border:3px solid black;border-radius:50%;width:46px;height:46px;display:flex;align-items:center;justify-content:center;"">🏪</div>',
    className: '',
    iconSize: [46,46],
    iconAnchor: [23,23]
}});

L.marker([{Txt(casaLat)}, {Txt(casaLon)}], {{icon: iconCasa{p.Id}}})
.addTo(map)
.bindPopup(`
<b>🏠 Casa del cliente</b><br>
<b>Pedido #{p.Id}</b><br>
{direccion}<br>
{detalles}<br>
Estado: <b>{status}</b><br>
{popupInfoRuta}<br>
Total: <b>{p.Total.ToString("C2")}</b><br><br>
<button onclick=""location.href='nontonio://select?id={p.Id}'""
style=""width:100%;padding:8px;border-radius:10px;background:#111;color:white;border:0;font-weight:bold;"">
Ver detalles
</button>
<button onclick=""location.href='nontonio://accept?id={p.Id}'""
style=""width:100%;padding:8px;border-radius:10px;background:#111;color:white;border:0;font-weight:bold;margin-top:6px;"">
Aceptar viaje
</button>
`);

L.marker([{Txt(tiendaLat)}, {Txt(tiendaLon)}], {{icon: iconTienda{p.Id}}})
.addTo(map)
.bindPopup('<b>🏪 {tienda}</b><br>Recoger pedido #{p.Id}');
";

                if (_mostrarRuta && _pedidoSeleccionado != null && _pedidoSeleccionado.Id == p.Id)
                {
                    if (ruta.ok && !string.IsNullOrWhiteSpace(ruta.polylineJs))
                    {
                        rutas += $@"
L.polyline({ruta.polylineJs}, {{
    color: 'black',
    weight: 5,
    opacity: 0.85
}}).addTo(map);
";
                    }
                    else
                    {
                        rutas += $@"
L.polyline([
    [{Txt(repartidorLat)}, {Txt(repartidorLon)}],
    [{Txt(tiendaLat)}, {Txt(tiendaLon)}],
    [{Txt(casaLat)}, {Txt(casaLon)}]
], {{
    color: 'black',
    weight: 5,
    opacity: 0.8,
    dashArray: '8,8'
}}).addTo(map);
";
                    }
                }
            }

            string html = $@"
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<style>
html, body, #map {{
    height:100%;
    margin:0;
    padding:0;
}}
.leaflet-popup-content button {{
    cursor:pointer;
}}
</style>
</head>
<body>
<div id='map'></div>

<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<script>
var map = L.map('map').setView([{Txt(baseLat)}, {Txt(baseLon)}], 15);

L.tileLayer('https://{{s}}.basemaps.cartocdn.com/light_all/{{z}}/{{x}}/{{y}}{{r}}.png', {{
    maxZoom: 19,
    subdomains: 'abcd'
}}).addTo(map);

{markers}
{rutas}

var puntos = [{bounds}];

if (puntos.length > 1) {{
    map.fitBounds(puntos, {{ padding: [70, 70] }});
}}
</script>
</body>
</html>";

            MapaPedidosWebView.Source = new HtmlWebViewSource { Html = html };
        }

        private async Task<(bool ok, double km, int min, string polylineJs)> ObtenerRutaOsrm(
            double repartidorLat, double repartidorLon,
            double tiendaLat, double tiendaLon,
            double casaLat, double casaLon)
        {
            try
            {
                string url =
                    $"https://router.project-osrm.org/route/v1/driving/" +
                    $"{Txt(repartidorLon)},{Txt(repartidorLat)};" +
                    $"{Txt(tiendaLon)},{Txt(tiendaLat)};" +
                    $"{Txt(casaLon)},{Txt(casaLat)}" +
                    $"?overview=full&geometries=geojson";

                string json = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var route = doc.RootElement.GetProperty("routes")[0];

                double distance = route.GetProperty("distance").GetDouble();
                double duration = route.GetProperty("duration").GetDouble();

                var coords = route.GetProperty("geometry").GetProperty("coordinates").EnumerateArray();

                List<string> points = new();

                foreach (var c in coords)
                {
                    double lon = c[0].GetDouble();
                    double lat = c[1].GetDouble();

                    points.Add($"[{Txt(lat)}, {Txt(lon)}]");
                }

                string js = "[" + string.Join(",", points) + "]";

                return (true, distance / 1000.0, Math.Max(1, (int)Math.Round(duration / 60.0)), js);
            }
            catch
            {
                return (false, 0, 0, "");
            }
        }

        private async void MapaPedidosWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("nontonio://", StringComparison.OrdinalIgnoreCase))
                return;

            e.Cancel = true;

            try
            {
                var uri = new Uri(e.Url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                if (!int.TryParse(query.Get("id"), out int id))
                    return;

                var orden = _pedidosActivos.FirstOrDefault(o => o.Id == id);

                if (orden == null)
                    return;

                if (uri.Host == "select")
                    SeleccionarPedido(orden);
                else if (uri.Host == "accept")
                    await AceptarPedido(orden);
            }
            catch { }
        }

        private void SeleccionarPedido(Orden orden)
        {
            _pedidoSeleccionado = orden;
            _mostrarRuta = false;

            PedidoInfoLabel.Text = $"Pedido #{orden.Id} • {orden.Total:C2}";
            PedidoDetalleLabel.Text =
                $"🏠 {orden.Direccion}\n" +
                $"🏪 {(string.IsNullOrWhiteSpace(orden.BranchName) ? "Tienda NONTONIO" : orden.BranchName)}\n" +
                $"📦 {orden.Detalles}\n" +
                $"Estado: {orden.Status}";

            VerRutaButton.Text = "Ver ruta";
            VerRutaButton.IsVisible = true;
            EntregadoButton.IsVisible = false;
        }

        private async Task AceptarPedido(Orden orden)
        {
            if (_simulandoEntrega)
                return;

            _pedidoSeleccionado = orden;
            _mostrarRuta = true;

            await CambiarEstado(orden, "Aceptado", false);

            PedidoInfoLabel.Text = $"🚚 Viaje aceptado • Pedido #{orden.Id}";
            PedidoDetalleLabel.Text = "Se abrirá la ruta animada hacia tienda y casa.";

            VerRutaButton.Text = "Mostrar ruta";
            VerRutaButton.IsVisible = true;
            EntregadoButton.IsVisible = true;

            await Navigation.PushAsync(new RepartidorRutaPage(orden, true));
        }

        private async Task CambiarEstado(Orden orden, string estado, bool sumarGanancia)
        {
            orden.Status = estado;
            orden.HistoryJson += $"{DateTime.Now:o}|{estado};";

            try
            {
                await OrdenesService.ActualizarOrdenAsync(orden);
            }
            catch
            {
                await OrdenesService.GuardarOrdenAsync(orden);
            }

            if (sumarGanancia)
                await SumarGanancia();

            await CargarPerfil();
            await CargarPedidos();
        }

        private async Task SumarGanancia()
        {
            var user = AuthService.GetCurrentUser();

            if (string.IsNullOrWhiteSpace(user))
                return;

            var perfil = await RepartidorService.ObtenerAsync(user);

            if (perfil == null)
                return;

            perfil.Viajes += 1;
            perfil.Saldo += 35;

            await RepartidorService.GuardarAsync(perfil);
        }

        private async void OnVerRutaSeleccionadaClicked(object sender, EventArgs e)
        {
            if (_pedidoSeleccionado == null)
            {
                await DisplayAlert("Pedido", "Selecciona un pedido en el mapa.", "OK");
                return;
            }

            _mostrarRuta = !_mostrarRuta;
            VerRutaButton.Text = _mostrarRuta ? "Ocultar ruta" : "Ver ruta";

            await CargarMapaPedidosAsync();
        }

        private static string Txt(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string JavaSafe(string value)
        {
            return (value ?? "")
                .Replace("\\", "\\\\")
                .Replace("`", "'")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("'", "\\'");
        }
    }
}