using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class RepartidorRegistroPage : ContentPage
    {
        private string _fotoPath = string.Empty;

        public RepartidorRegistroPage()
        {
            InitializeComponent();
        }

        private async void OnSeleccionarFotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                if (photo == null)
                    return;

                var fileName = $"repartidor_{DateTime.Now.Ticks}.jpg";
                var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using var stream = await photo.OpenReadAsync();
                using var newStream = File.OpenWrite(localPath);
                await stream.CopyToAsync(newStream);

                _fotoPath = localPath;
                FotoPreview.Source = ImageSource.FromFile(localPath);
            }
            catch
            {
                await DisplayAlert("Foto", "No se pudo seleccionar la foto.", "OK");
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            var user = AuthService.GetCurrentUser();

            if (string.IsNullOrWhiteSpace(user))
            {
                await DisplayAlert("Error", "No hay usuario iniciado.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                string.IsNullOrWhiteSpace(TelefonoEntry.Text) ||
                string.IsNullOrWhiteSpace(PlacasEntry.Text) ||
                string.IsNullOrWhiteSpace(IneEntry.Text) ||
                string.IsNullOrWhiteSpace(DomicilioEntry.Text))
            {
                await DisplayAlert("Faltan datos", "Completa todos los campos.", "OK");
                return;
            }

            var profile = new RepartidorProfile
            {
                Username = user,
                Nombre = NombreEntry.Text.Trim(),
                Telefono = TelefonoEntry.Text.Trim(),
                Placas = PlacasEntry.Text.Trim(),
                Identificacion = IneEntry.Text.Trim(),
                Domicilio = DomicilioEntry.Text.Trim(),
                FotoPath = _fotoPath,
                Saldo = 0,
                Viajes = 0
            };

            await RepartidorService.GuardarAsync(profile);

            Application.Current!.Windows[0].Page =
                new NavigationPage(new RepartidorPage());
        }
    }
}