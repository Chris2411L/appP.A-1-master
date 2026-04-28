using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class LoginPage : ContentPage
    {
        private const string AdminUser = "admin";
        private const string AdminPass = "123456";
        private int _captchaResult;

        public LoginPage()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoginCard.Opacity = 0;
            LoginCard.TranslationY = 25;
            HeroImage.Scale = 1.05;
            LogoIcon.Opacity = 0;

            await Task.WhenAll(
                LoginCard.FadeTo(1, 500, Easing.CubicOut),
                LoginCard.TranslateTo(0, 0, 500, Easing.CubicOut),
                HeroImage.ScaleTo(1.12, 6000, Easing.Linear),
                LogoIcon.FadeTo(1, 600, Easing.CubicOut)
            );

            _ = FloatingCardAnimation();
        }

        private async Task FloatingCardAnimation()
        {
            while (LoginCard != null)
            {
                await LoginCard.TranslateTo(0, -8, 1800, Easing.SinInOut);
                await LoginCard.TranslateTo(0, 0, 1800, Easing.SinInOut);
            }
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int a = rnd.Next(1, 10);
            int b = rnd.Next(1, 10);

            _captchaResult = a + b;
            CaptchaLabel.Text = $"{a} + {b}";
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
            await LoginButton.ScaleTo(0.96, 80, Easing.CubicOut);
        }

        private async void OnButtonReleased(object sender, EventArgs e)
        {
            await LoginButton.ScaleTo(1, 120, Easing.CubicOut);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string user = UsernameEntry.Text?.Trim() ?? string.Empty;
            string pass = PasswordEntry.Text ?? string.Empty;
            string captcha = CaptchaEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                ShowMessage("Ingresa usuario y contraseña.");
                return;
            }

            if (captcha != _captchaResult.ToString())
            {
                ShowMessage("Captcha incorrecto.");
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