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
            WelcomeLabel.Text = string.IsNullOrEmpty(user) ? "¡Bienvenido!" : $"¡Hola, {user}!";

            bool isAdmin = AppData.IsAdmin;
            AdminPanel.IsVisible = isAdmin;
            ChangeUserButton.IsVisible = !string.IsNullOrEmpty(user) || isAdmin;

            try
            {
                await InventarioService.SincronizarStockAsync();
            }
            catch { }

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

        private async Task AnimateHeroImage()
        {
            while (_runningHeroAnimation)
            {
                await HeroImage.ScaleTo(1.08, 6000, Easing.SinInOut);
                await HeroImage.ScaleTo(1.03, 6000, Easing.SinInOut);
            }
        }

        private async void OnVerCategoriasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoriasPage());
        }

        private async void OnVerDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DashboardPage());
        }

        private async void OnVerCarritoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CarritoPage());
        }

        private async void OnVerPerfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MisComprasPage());
        }

        private async void OnPanelUsuariosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UsuariosAdminPage());
        }

        private async void OnPanelAlmacenClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AlmacenAdminPage());
        }

        private async void OnPanelOrdenesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersAdminPage());
        }

        private void OnChangeUserClicked(object sender, EventArgs e)
        {
            AuthService.Logout();
            AppData.IsAdmin = false;
            Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
        }

        private async void OnCategoriasCardTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new CategoriasPage());
        }

        private async void OnDashboardCardTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new DashboardPage());
        }

        private async void OnPerfilCardTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new MisComprasPage());
        }

        private async void OnAddCategoryClicked(object sender, EventArgs e)
        {
            string nombre = await DisplayPromptAsync("Admin", "Nombre de la nueva categoría:");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                AppData.Categorias.Add(new Categoria(nombre.Trim(), ""));
                await DisplayAlert("Éxito", "Categoría guardada", "OK");
            }
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            OnChangeUserClicked(sender, e);
        }
    }
}