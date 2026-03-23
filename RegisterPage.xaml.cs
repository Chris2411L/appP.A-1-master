using appP.A.Services;

namespace appP.A
{
    public partial class RegisterPage : ContentPage
    {
        private int _captchaResult;

        public RegisterPage()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int val1 = rnd.Next(1, 15);
            int val2 = rnd.Next(1, 15);
            _captchaResult = val1 + val2;
            CaptchaLabel.Text = $"{val1} + {val2} =";
            CaptchaEntry.Text = string.Empty;
        }

        private void OnRefreshCaptchaClicked(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        private void ShowMessage(string text, Color color)
        {
            MessageLabel.Text = text;
            MessageLabel.TextColor = color;
            MessageLabel.IsVisible = true;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var user = UsernameEntry.Text?.Trim() ?? string.Empty;
            var pass = PasswordEntry.Text ?? string.Empty;
            var confirmPass = ConfirmPasswordEntry.Text ?? string.Empty;
            var captchaInput = CaptchaEntry.Text?.Trim() ?? string.Empty;

            // 1. Validar campos vacíos
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                ShowMessage("Completa todos los campos.", Colors.Red);
                return;
            }

            // 2. Validar que las contraseñas coincidan
            if (pass != confirmPass)
            {
                ShowMessage("Las contraseñas no coinciden.", Colors.Red);
                return;
            }

            // 3. Validar Captcha
            if (captchaInput != _captchaResult.ToString())
            {
                ShowMessage("Captcha incorrecto.", Colors.Red);
                GenerateCaptcha();
                return;
            }

            // 4. Intentar registro
            var (success, error) = await AuthService.RegisterAsync(user, pass);
            if (success)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente.", "Aceptar");
                await Navigation.PopAsync(); // Regresar al Login
            }
            else
            {
                ShowMessage(string.IsNullOrWhiteSpace(error) ? "El usuario ya existe o hubo un error." : error, Colors.Red);
                GenerateCaptcha();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}