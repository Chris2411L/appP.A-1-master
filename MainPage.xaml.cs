using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 1. Actualizar Texto de Bienvenida
            var user = AuthService.GetCurrentUser();
            WelcomeLabel.Text = string.IsNullOrEmpty(user) ? "¡Bienvenido!" : $"¡Hola, {user}!";

            // 2. Control de botones según si es Admin o no
            bool isAdmin = AppData.IsAdmin;
            AdminPanel.IsVisible = isAdmin;
            ChangeUserButton.IsVisible = !string.IsNullOrEmpty(user) || isAdmin;

            // 3. Sincronizar Stock
            try { await InventarioService.SincronizarStockAsync(); } catch { }
        }

        // Navegación básica
        private async void OnVerCategoriasClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CategoriasPage());
        private async void OnVerCarritoClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CarritoPage());
        private async void OnVerPerfilClicked(object sender, EventArgs e) => await Navigation.PushAsync(new MisComprasPage());

        // Admin
        private async void OnPanelUsuariosClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UsuariosAdminPage());
        private async void OnPanelAlmacenClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AlmacenAdminPage());

        private void OnChangeUserClicked(object sender, EventArgs e)
        {
            AuthService.Logout();
            AppData.IsAdmin = false;
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        // Si necesitas agregar categorías desde aquí
        private async void OnAddCategoryClicked(object sender, EventArgs e)
        {
            string nombre = await DisplayPromptAsync("Admin", "Nombre de la nueva categoría:");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                AppData.Categorias.Add(new Categoria(nombre.Trim(), ""));
                await DisplayAlert("Éxito", "Categoría guardada", "OK");
            }
        }

        private void OnLogoutClicked(object sender, EventArgs e) => OnChangeUserClicked(sender, e);
    }
}