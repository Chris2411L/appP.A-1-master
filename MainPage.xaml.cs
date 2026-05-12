using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class MainPage : ContentPage
    {
        private bool _runningHeroAnimation;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var user = AuthService.GetCurrentUser();
            var rol = await AuthService.GetCurrentUserRoleAsync();

            WelcomeLabel.Text = string.IsNullOrEmpty(user)
                ? "¡Bienvenido!"
                : $"¡Hola, {user}!";

            // PANEL VENDEDOR
            AdminPanel.IsVisible = rol == "Vendedor";

            // BOTÓN SALIR
            ChangeUserButton.IsVisible =
                !string.IsNullOrEmpty(user);

            try
            {
                await InventarioService.SincronizarStockAsync();
            }
            catch
            {
            }

            // ANIMACIONES
            MainContent.Opacity = 0;
            MainContent.TranslationY = 24;

            HeroCard.Opacity = 0;
            HeroCard.Scale = 0.97;

            QuickActions.Opacity = 0;
            QuickActions.TranslationY = 18;

            HeroImage.Scale = 1.03;

            await Task.WhenAll(
                MainContent.FadeTo(1, 420, Easing.CubicOut),
                MainContent.TranslateTo(0, 0, 420, Easing.CubicOut),

                HeroCard.FadeTo(1, 520, Easing.CubicOut),
                HeroCard.ScaleTo(1, 520, Easing.CubicOut),

                QuickActions.FadeTo(1, 650, Easing.CubicOut),
                QuickActions.TranslateTo(0, 0, 650, Easing.CubicOut)
            );

            _runningHeroAnimation = true;
            _ = AnimateHeroImage();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _runningHeroAnimation = false;
        }

        // =========================================
        // ANIMACIÓN HERO
        // =========================================
        private async Task AnimateHeroImage()
        {
            while (_runningHeroAnimation)
            {
                await HeroImage.ScaleTo(
                    1.08,
                    6000,
                    Easing.SinInOut);

                if (!_runningHeroAnimation)
                    break;

                await HeroImage.ScaleTo(
                    1.03,
                    6000,
                    Easing.SinInOut);
            }
        }

        // =========================================
        // ENTRAR MODO VENDEDOR
        // =========================================
        private async void OnLogoSecretTapped(
            object sender,
            TappedEventArgs e)
        {
            var rol =
                await AuthService.GetCurrentUserRoleAsync();

            if (rol != "Vendedor")
                return;

            Application.Current!.Windows[0].Page =
                new NavigationPage(
                    new VendedorDashboardPage());
        }

        // =========================================
        // IA NONTONIO
        // =========================================
        private async void OnAIButtonClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new AIStoreAssistantPage());
        }

        // =========================================
        // BOTONES PRINCIPALES
        // =========================================
        private async void OnVerCategoriasClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new CategoriasPage());
        }

        private async void OnVerDashboardClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new DashboardPage());
        }

        private async void OnVerCarritoClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new CarritoPage());
        }

        private async void OnVerPerfilClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new MisComprasPage());
        }

        // =========================================
        // TARJETAS HOME
        // =========================================
        private async void OnCategoriasCardTapped(
            object sender,
            TappedEventArgs e)
        {
            await Navigation.PushAsync(
                new CategoriasPage());
        }

        private async void OnDashboardCardTapped(
            object sender,
            TappedEventArgs e)
        {
            await Navigation.PushAsync(
                new DashboardPage());
        }

        private async void OnPerfilCardTapped(
            object sender,
            TappedEventArgs e)
        {
            await Navigation.PushAsync(
                new MisComprasPage());
        }

        // =========================================
        // PANEL VENDEDOR
        // =========================================
        private async void OnPanelUsuariosClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new UsuariosAdminPage());
        }

        private async void OnPanelAlmacenClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new AlmacenAdminPage());
        }

        private async void OnPanelOrdenesClicked(
            object sender,
            EventArgs e)
        {
            await Navigation.PushAsync(
                new OrdersAdminPage());
        }

        // =========================================
        // AGREGAR CATEGORÍA
        // =========================================
        private async void OnAddCategoryClicked(
            object sender,
            EventArgs e)
        {
            string nombre =
                await DisplayPromptAsync(
                    "Nueva categoría",
                    "Nombre:");

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                AppData.Categorias.Add(
                    new Categoria(
                        nombre.Trim(),
                        "📦"));

                await DisplayAlert(
                    "Éxito",
                    "Categoría agregada",
                    "OK");
            }
        }

        // =========================================
        // LOGOUT
        // =========================================
        private void OnLogoutClicked(
            object sender,
            EventArgs e)
        {
            OnChangeUserClicked(sender, e);
        }

        private void OnChangeUserClicked(
            object sender,
            EventArgs e)
        {
            AuthService.Logout();

            Application.Current!.Windows[0].Page =
                new NavigationPage(
                    new LoginPage());
        }
    }
}