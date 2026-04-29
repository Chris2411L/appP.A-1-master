using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class LoginPage : ContentPage
    {
        private const string AdminUser = "admin";
        private const string AdminPass = "123456";

        private int _captchaResult;
        private bool _runningAnimation;
        private bool _yaAnimado = false;

        public LoginPage()
        {
            InitializeComponent();
            GenerateCaptcha();
            SizeChanged += OnPageSizeChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ApplyResponsiveLayout(Width);

            if (_yaAnimado)
            {
                _runningAnimation = true;
                _ = AnimateHeroImage();
                _ = AnimateLoginCard();
                return;
            }

            _yaAnimado = true;

            LoginCard.Opacity = 0;
            LoginCard.TranslationY = 24;
            HeroImage.Scale = 1.02;
            LogoIcon.Opacity = 0;

            // Animaciones secuenciales y más cortas
            await LoginCard.FadeTo(1, 280, Easing.CubicOut);
            await LoginCard.TranslateTo(0, 0, 280, Easing.CubicOut);
            await LogoIcon.FadeTo(1, 300, Easing.CubicOut);

            _runningAnimation = true;
            _ = AnimateHeroImage();
            _ = AnimateLoginCard();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _runningAnimation = false;
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout(Width);
        }

        private void ApplyResponsiveLayout(double width)
        {
            if (width <= 0) return;

            ResponsiveGrid.ColumnDefinitions.Clear();
            ResponsiveGrid.RowDefinitions.Clear();

            if (width < 700)
            {
                ResponsiveGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                ResponsiveGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                ResponsiveGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                Grid.SetColumn(LoginPanel, 0);
                Grid.SetRow(LoginPanel, 1);

                HeroPanel.MinimumHeightRequest = 260;
                LoginPanel.MinimumHeightRequest = 520;

                LoginCard.WidthRequest = -1;
                LoginCard.MaximumWidthRequest = 390;
                LoginCard.HorizontalOptions = LayoutOptions.Fill;

                HeroText.Padding = new Thickness(26);
            }
            else if (width < 1050)
            {
                ResponsiveGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                ResponsiveGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                ResponsiveGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                Grid.SetColumn(LoginPanel, 0);
                Grid.SetRow(LoginPanel, 1);

                HeroPanel.MinimumHeightRequest = 320;
                LoginPanel.MinimumHeightRequest = 560;

                LoginCard.WidthRequest = 420;
                LoginCard.MaximumWidthRequest = 440;
                LoginCard.HorizontalOptions = LayoutOptions.Center;

                HeroText.Padding = new Thickness(40);
            }
            else
            {
                ResponsiveGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                ResponsiveGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                ResponsiveGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                Grid.SetColumn(LoginPanel, 1);
                Grid.SetRow(LoginPanel, 0);

                HeroPanel.MinimumHeightRequest = 720;
                LoginPanel.MinimumHeightRequest = 720;

                LoginCard.WidthRequest = 360;
                LoginCard.MaximumWidthRequest = 420;
                LoginCard.HorizontalOptions = LayoutOptions.Center;

                HeroText.Padding = new Thickness(50);
            }
        }

        private async Task AnimateHeroImage()
        {
            while (_runningAnimation)
            {
                await HeroImage.ScaleTo(1.05, 1800, Easing.SinInOut);
                if (!_runningAnimation) break;

                await HeroImage.ScaleTo(1.02, 1800, Easing.SinInOut);
            }
        }

        private async Task AnimateLoginCard()
        {
            while (_runningAnimation)
            {
                await LoginCard.TranslateTo(0, -3, 1200, Easing.SinInOut);
                if (!_runningAnimation) break;

                await LoginCard.TranslateTo(0, 0, 1200, Easing.SinInOut);
            }
        }

        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
                await entry.ScaleTo(1.01, 80, Easing.CubicOut);
        }

        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
                await entry.ScaleTo(1.0, 80, Easing.CubicOut);
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int val1 = rnd.Next(1, 10);
            int val2 = rnd.Next(1, 10);

            _captchaResult = val1 + val2;
            CaptchaLabel.Text = $"{val1} + {val2} =";
            CaptchaEntry.Text = string.Empty;
        }

        private void OnRefreshCaptchaClicked(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        private void ShowMessage(string text)
        {
            MessageLabel.Text = text;
            MessageLabel.IsVisible = true;
        }

        private async void OnButtonPressed(object sender, EventArgs e)
        {
            await LoginButton.ScaleTo(0.96, 70, Easing.CubicOut);
        }

        private async void OnButtonReleased(object sender, EventArgs e)
        {
            await LoginButton.ScaleTo(1, 90, Easing.CubicOut);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string user = UsernameEntry.Text?.Trim() ?? string.Empty;
            string pass = PasswordEntry.Text ?? string.Empty;
            string captchaInput = CaptchaEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                ShowMessage("Por favor, ingresa tus credenciales.");
                return;
            }

            if (captchaInput != _captchaResult.ToString())
            {
                ShowMessage("La suma es incorrecta.");
                GenerateCaptcha();
                return;
            }

            if (user.Equals(AdminUser, StringComparison.OrdinalIgnoreCase) && pass == AdminPass)
            {
                AppData.IsAdmin = true;
                Application.Current!.Windows[0].Page = new AppShell();
                return;
            }

            bool ok = await AuthService.LoginAsync(user, pass);

            if (ok)
            {
                AppData.IsAdmin = false;
                Application.Current!.Windows[0].Page = new AppShell();
            }
            else
            {
                ShowMessage("Usuario o contraseña incorrectos.");
                GenerateCaptcha();
            }
        }

        private async void OnRegisterTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}