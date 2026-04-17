using System;
using System.Threading.Tasks;
using appP.A.Models;
using appP.A.Services;
using Microsoft.Maui.ApplicationModel;

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
            this.Opacity = 0;
            await this.FadeTo(1, 320, Easing.CubicOut);
        }

        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                await entry.ScaleTo(1.02, 120, Easing.CubicOut);
            }
        }

        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                await entry.ScaleTo(1.0, 120, Easing.CubicOut);
            }
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
            MessageFrame.IsVisible = true;
            MessageFrame.FadeTo(1, 180);
            // auto hide
            _ = Task.Run(async () =>
            {
                await Task.Delay(3200);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await MessageFrame.FadeTo(0, 220);
                    MessageFrame.IsVisible = false;
                });
            });
        }

        private async void OnButtonPressed(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                await b.ScaleTo(0.98, 80, Easing.CubicOut);
            }
        }

        private async void OnButtonReleased(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                await b.ScaleTo(1.0, 120, Easing.CubicOut);
            }
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