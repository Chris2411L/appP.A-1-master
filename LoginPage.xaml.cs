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

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string user = UsernameEntry.Text ?? string.Empty;
            string pass = PasswordEntry.Text ?? string.Empty;
            string captchaInput = CaptchaEntry.Text ?? string.Empty;

            user = user.Trim();
            captchaInput = captchaInput.Trim();

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
                MessagingCenter.Send(this, "AdminModeChanged", true);
                Application.Current.MainPage = new AppShell();
                return;
            }

            bool ok = await AuthService.LoginAsync(user, pass);
            if (ok)
            {
                AppData.IsAdmin = false;
                MessagingCenter.Send(this, "AdminModeChanged", false);
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                ShowMessage("Usuario o contraseña incorrectos.");
                GenerateCaptcha();
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}