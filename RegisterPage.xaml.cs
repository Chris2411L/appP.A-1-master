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

        protected override async void OnAppearing()
        {            
            base.OnAppearing();

            RegisterCard.Opacity = 0;
            RegisterCard.TranslationY = 35;
            HeroImage.Scale = 1.04;

            await Task.WhenAll(
                RegisterCard.FadeTo(1, 450, Easing.CubicOut),
                RegisterCard.TranslateTo(0, 0, 450, Easing.CubicOut),
                HeroImage.ScaleTo(1.10, 7000, Easing.Linear)
            );
        }

        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
                await entry.ScaleTo(1.02, 120, Easing.CubicOut);
        }

        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
                await entry.ScaleTo(1.0, 120, Easing.CubicOut);
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int a = rnd.Next(1, 15);
            int b = rnd.Next(1, 15);

            _captchaResult = a + b;
            CaptchaLabel.Text = $"{a} + {b} =";
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
            await RegisterButton.ScaleTo(0.96, 80, Easing.CubicOut);
        }

        private async void OnButtonReleased(object sender, EventArgs e)
        {
            await RegisterButton.ScaleTo(1, 120, Easing.CubicOut);
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var user = UsernameEntry.Text?.Trim() ?? string.Empty;
            var pass = PasswordEntry.Text ?? string.Empty;
            var confirmPass = ConfirmPasswordEntry.Text ?? string.Empty;
            var captchaInput = CaptchaEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                ShowMessage("Completa todos los campos.");
                return;
            }

            if (pass != confirmPass)
            {
                ShowMessage("Las contraseñas no coinciden.");
                return;
            }

            if (captchaInput != _captchaResult.ToString())
            {
                ShowMessage("Captcha incorrecto.");
                GenerateCaptcha();
                return;
            }

            var (success, error) = await AuthService.RegisterAsync(user, pass);

            if (success)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente.", "Aceptar");
                await Navigation.PopAsync();
            }
            else
            {
                ShowMessage(string.IsNullOrWhiteSpace(error)
                    ? "El usuario ya existe o hubo un error."
                    : error);

                GenerateCaptcha();
            }
        }

        private async void OnBackTextTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });

            return true;
        }
    }
}
