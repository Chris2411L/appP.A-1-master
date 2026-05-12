// Importa los servicios del proyecto
using appP.A.Services;

// Namespace principal del proyecto
namespace appP.A
{
    // Clase parcial RegisterPage que hereda de ContentPage
    // Representa la pantalla de registro
    public partial class RegisterPage : ContentPage
    {
        // Variable que guarda el resultado correcto del captcha
        private int _captchaResult;

        // Constructor principal
        public RegisterPage()
        {
            // Carga todos los componentes visuales del XAML
            InitializeComponent();

            // Genera un captcha automáticamente
            GenerateCaptcha();

            // Selecciona el primer elemento del Picker por defecto
            // En este caso probablemente "Cliente"
            RolePicker.SelectedIndex = 0;
        }

        // Método que se ejecuta cuando la página aparece
        protected override async void OnAppearing()
        {
            // Ejecuta comportamiento original de ContentPage
            base.OnAppearing();

            // Hace invisible la tarjeta de registro
            RegisterCard.Opacity = 0;

            // Baja la tarjeta 35 pixeles
            RegisterCard.TranslationY = 35;

            // Escala ligeramente la imagen
            HeroImage.Scale = 1.04;

            // Ejecuta varias animaciones al mismo tiempo
            await Task.WhenAll(

                // Hace aparecer la tarjeta
                RegisterCard.FadeTo(1, 450, Easing.CubicOut),

                // Mueve tarjeta hacia arriba
                RegisterCard.TranslateTo(0, 0, 450, Easing.CubicOut),

                // Hace zoom lento a la imagen
                HeroImage.ScaleTo(1.10, 7000, Easing.Linear)
            );
        }

        // Evento cuando un Entry obtiene foco
        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            // Verifica que el sender sea Entry
            if (sender is Entry entry)

                // Agranda ligeramente el textbox
                await entry.ScaleTo(1.02, 120, Easing.CubicOut);
        }

        // Evento cuando un Entry pierde foco
        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            // Verifica que sea Entry
            if (sender is Entry entry)

                // Regresa tamaño normal
                await entry.ScaleTo(1.0, 120, Easing.CubicOut);
        }

        // Método que genera captcha aleatorio
        private void GenerateCaptcha()
        {
            // Generador de números aleatorios
            Random rnd = new Random();

            // Número aleatorio entre 1 y 14
            int a = rnd.Next(1, 15);

            // Segundo número aleatorio
            int b = rnd.Next(1, 15);

            // Guarda resultado correcto
            _captchaResult = a + b;

            // Muestra operación en pantalla
            CaptchaLabel.Text = $"{a} + {b} =";

            // Limpia textbox del captcha
            CaptchaEntry.Text = string.Empty;
        }

        // Evento botón refrescar captcha
        private void OnRefreshCaptchaClicked(object sender, EventArgs e)
        {
            // Genera nuevo captcha
            GenerateCaptcha();
        }

        // Método para mostrar mensajes de error o información
        private void ShowMessage(string text)
        {
            // Cambia el texto del Label
            MessageLabel.Text = text;

            // Hace visible el mensaje
            MessageLabel.IsVisible = true;
        }

        // Evento cuando presionan el botón registrar
        private async void OnButtonPressed(object sender, EventArgs e)
        {
            // Hace pequeño el botón para efecto visual
            await RegisterButton.ScaleTo(0.96, 80, Easing.CubicOut);
        }

        // Evento cuando sueltan el botón registrar
        private async void OnButtonReleased(object sender, EventArgs e)
        {
            // Regresa botón a tamaño normal
            await RegisterButton.ScaleTo(1, 120, Easing.CubicOut);
        }

        // Evento principal de registro
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Obtiene nombre usuario
            var user = UsernameEntry.Text?.Trim() ?? string.Empty;

            // Obtiene contraseña
            var pass = PasswordEntry.Text ?? string.Empty;

            // Obtiene confirmación contraseña
            var confirmPass = ConfirmPasswordEntry.Text ?? string.Empty;

            // Obtiene captcha ingresado
            var captchaInput = CaptchaEntry.Text?.Trim() ?? string.Empty;

            // Obtiene rol seleccionado
            // Si no hay rol usa "Cliente"
            var rol = RolePicker.SelectedItem?.ToString() ?? "Cliente";

            // Verifica campos vacíos
            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                // Muestra mensaje error
                ShowMessage("Completa todos los campos.");
                return;
            }

            // Verifica si las contraseñas coinciden
            if (pass != confirmPass)
            {
                // Error contraseñas
                ShowMessage("Las contraseñas no coinciden.");
                return;
            }

            // Verifica si seleccionó rol
            if (RolePicker.SelectedIndex == -1)
            {
                // Error selección
                ShowMessage("Selecciona si eres Cliente, Repartidor o Vendedor.");
                return;
            }

            // Verifica captcha
            if (captchaInput != _captchaResult.ToString())
            {
                // Error captcha
                ShowMessage("Captcha incorrecto.");

                // Genera nuevo captcha
                GenerateCaptcha();
                return;
            }

            // Llama servicio de registro
            // Devuelve success y error
            var (success, error) =
                await AuthService.RegisterAsync(user, pass, rol);

            // Si el registro fue exitoso
            if (success)
            {
                // Muestra alerta éxito
                await DisplayAlert(
                    "Éxito",
                    $"Usuario registrado como {rol}.",
                    "Aceptar");

                // Regresa a página anterior
                await Navigation.PopAsync();
            }
            else
            {
                // Muestra error personalizado
                ShowMessage(

                    // Si error viene vacío
                    string.IsNullOrWhiteSpace(error)

                    // Mensaje genérico
                    ? "El usuario ya existe o hubo un error."

                    // Si sí existe error muestra ese
                    : error
                );

                // Genera nuevo captcha
                GenerateCaptcha();
            }
        }

        // Evento al tocar texto "volver"
        private async void OnBackTextTapped(
            object sender,
            TappedEventArgs e)
        {
            // Regresa página anterior
            await Navigation.PopAsync();
        }

        // Evento botón volver
        private async void OnBackClicked(
            object sender,
            EventArgs e)
        {
            // Regresa página anterior
            await Navigation.PopAsync();
        }

        // Evento botón físico atrás Android
        protected override bool OnBackButtonPressed()
        {
            // Ejecuta navegación en hilo principal
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Regresa página anterior
                await Navigation.PopAsync();
            });

            // Cancela comportamiento normal del botón atrás
            return true;
        }
    }
}