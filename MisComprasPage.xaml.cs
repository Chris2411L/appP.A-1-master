using appP.A.Models;
using appP.A.Services;
using System.Globalization;
using System.Text.Json;

namespace appP.A
{
    public partial class MisComprasPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private List<string> _direccionesNombres = new List<string>();

        public MisComprasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            CargarHistorial();
            CargarDirecciones();
        }

        private void OnTabDireccionesClicked(object sender, EventArgs e)
        {
            SeccionDirecciones.IsVisible = true;
            SeccionCompras.IsVisible = false;
            TabDireccionesFrame.BackgroundColor = Color.FromArgb("#8A2BE2");
            TabComprasFrame.BackgroundColor = Colors.Gray;
        }

        private void OnTabComprasClicked(object sender, EventArgs e)
        {
            SeccionDirecciones.IsVisible = false;
            SeccionCompras.IsVisible = true;
            TabComprasFrame.BackgroundColor = Color.FromArgb("#8A2BE2");
            TabDireccionesFrame.BackgroundColor = Colors.Gray;
        }

        private void CargarDirecciones()
        {
            var user = AuthService.GetCurrentUser() ?? "invitado";
            string lista = Preferences.Get($"lista_direcciones_{user}", "");
            if (!string.IsNullOrEmpty(lista))
            {
                _direccionesNombres = lista.Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
                SavedAddressesPicker.ItemsSource = null;
                SavedAddressesPicker.ItemsSource = _direccionesNombres;
            }
        }

        private void OnSavedAddressChanged(object sender, EventArgs e)
        {
            if (SavedAddressesPicker.SelectedIndex == -1) return;
            var user = AuthService.GetCurrentUser() ?? "invitado";
            string key = SavedAddressesPicker.SelectedItem.ToString();

            NombreDireccionEntry.Text = key;
            CalleEntry.Text = Preferences.Get($"{user}_{key}_calle", "");
            NumeroEntry.Text = Preferences.Get($"{user}_{key}_num", "");
            ColoniaEntry.Text = Preferences.Get($"{user}_{key}_col", "");
            CPEntry.Text = Preferences.Get($"{user}_{key}_cp", "");
            CiudadEntry.Text = Preferences.Get($"{user}_{key}_ciu", "");
            ActualizarMapa();
        }

        private async void OnSaveAddressClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreDireccionEntry.Text)) return;
            var user = AuthService.GetCurrentUser() ?? "invitado";
            string nombre = NombreDireccionEntry.Text.Trim();

            Preferences.Set($"{user}_{nombre}_calle", CalleEntry.Text);
            Preferences.Set($"{user}_{nombre}_num", NumeroEntry.Text);
            Preferences.Set($"{user}_{nombre}_col", ColoniaEntry.Text);
            Preferences.Set($"{user}_{nombre}_cp", CPEntry.Text);
            Preferences.Set($"{user}_{nombre}_ciu", CiudadEntry.Text);

            if (!_direccionesNombres.Contains(nombre))
            {
                _direccionesNombres.Add(nombre);
                Preferences.Set($"lista_direcciones_{user}", string.Join("|", _direccionesNombres));
            }
            CargarDirecciones();
            await DisplayAlert("Éxito", "Dirección guardada", "OK");
        }

        private async void OnDeleteAddressClicked(object sender, EventArgs e)
        {
            if (SavedAddressesPicker.SelectedIndex == -1) return;
            string nombre = SavedAddressesPicker.SelectedItem.ToString();
            if (await DisplayAlert("Eliminar", $"¿Borrar {nombre}?", "Sí", "No"))
            {
                var user = AuthService.GetCurrentUser() ?? "invitado";
                _direccionesNombres.Remove(nombre);
                Preferences.Set($"lista_direcciones_{user}", string.Join("|", _direccionesNombres));
                CargarDirecciones();
                NombreDireccionEntry.Text = "";
                CalleEntry.Text = "";
            }
        }

        private async void OnCPTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue?.Length == 5)
            {
                try
                {
                    var res = await client.GetStringAsync($"https://api.zippopotam.us/mx/{e.NewTextValue}");
                    using var doc = JsonDocument.Parse(res);
                    var place = doc.RootElement.GetProperty("places")[0];
                    CiudadEntry.Text = place.GetProperty("state").GetString();
                    ColoniaEntry.Text = place.GetProperty("place name").GetString();
                    ActualizarMapa();
                }
                catch { }
            }
        }

        private async void ActualizarMapa()
        {
            if (string.IsNullOrWhiteSpace(CalleEntry.Text)) return;
            try
            {
                string query = Uri.EscapeDataString($"{CalleEntry.Text} {NumeroEntry.Text}, {ColoniaEntry.Text}, {CiudadEntry.Text}, Mexico");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "NontonioApp");
                var res = await client.GetStringAsync($"https://nominatim.openstreetmap.org/search?format=json&q={query}&limit=1");

                using var doc = JsonDocument.Parse(res);
                // CORRECCIÓN AQUÍ: Se añade .RootElement antes de EnumerateArray
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

        private async void CargarHistorial()
        {
            var user = AuthService.GetCurrentUser() ?? "invitado";
            OrdenesList.ItemsSource = await OrdenesService.ObtenerOrdenesUsuarioAsync(user);
        }

        private async void OnOrdenSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Orden o)
            {
                ((CollectionView)sender).SelectedItem = null;
                string ticket = $"TICKET DE COMPRA\n----------\nFecha: {o.Fecha:g}\nDestino: {o.Direccion}\nPago: {o.MetodoPago}\n----------\nArtículos:\n{o.Detalles.Replace(", ", "\n")}\n----------\nTOTAL: {o.Total:C2}";
                await DisplayAlert("Detalles del Pedido", ticket, "OK");
            }
        }
    }
}