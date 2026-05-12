// Importa clases de modelos y servicios del proyecto
using appP.A.Models;
using appP.A.Services;

// Namespace principal del proyecto
namespace appP.A
{
    // Clase parcial LoginPage que hereda de ContentPage
    // ContentPage representa una página visual en .NET MAUI
    public partial class LoginPage : ContentPage
    {
        // Usuario administrador fijo
        private const string AdminUser = "admin";

        // Contraseña administrador fija
        private const string AdminPass = "123456";

        // Guarda el resultado correcto del captcha
        private int _captchaResult;

        // Controla si las animaciones siguen activas
        private bool _runningAnimation;

        // Evita repetir la animación inicial varias veces
        private bool _yaAnimado = false;

        // Constructor principal de la página
        public LoginPage()
        {
            // Carga todos los componentes del XAML
            InitializeComponent();

            // Genera un captcha al abrir la página
            GenerateCaptcha();

            // Evento que detecta cambios de tamaño en la ventana
            SizeChanged += OnPageSizeChanged;
        }

        // Método que se ejecuta cuando la página aparece
        protected override async void OnAppearing()
        {
            // Ejecuta comportamiento base de ContentPage
            base.OnAppearing();

            // Aplica diseño responsive dependiendo del ancho
            ApplyResponsiveLayout(Width);

            // Si ya se animó anteriormente
            if (_yaAnimado)
            {
                // Activa animaciones
                _runningAnimation = true;

                // Ejecuta animación de imagen
                _ = AnimateHeroImage();

                // Ejecuta animación de tarjeta login
                _ = AnimateLoginCard();

                return;
            }

            // Marca que ya se ejecutó la animación inicial
            _yaAnimado = true;

            // Hace invisible la tarjeta login
            LoginCard.Opacity = 0;

            // Baja la tarjeta 24 pixeles
            LoginCard.TranslationY = 24;

            // Escala ligeramente la imagen principal
            HeroImage.Scale = 1.02;

            // Oculta el logo inicialmente
            LogoIcon.Opacity = 0;

            // Hace aparecer la tarjeta suavemente
            await LoginCard.FadeTo(1, 280, Easing.CubicOut);

            // Mueve la tarjeta hacia arriba
            await LoginCard.TranslateTo(0, 0, 280, Easing.CubicOut);

            // Hace aparecer el logo
            await LogoIcon.FadeTo(1, 300, Easing.CubicOut);

            // Activa animaciones continuas
            _runningAnimation = true;

            // Inicia animaciones
            _ = AnimateHeroImage();
            _ = AnimateLoginCard();
        }

        // Método que se ejecuta cuando sales de la página
        protected override void OnDisappearing()
        {
            // Ejecuta comportamiento base
            base.OnDisappearing();

            // Detiene animaciones
            _runningAnimation = false;
        }

        // Evento que detecta cambio de tamaño de pantalla
        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            // Reaplica diseño responsive
            ApplyResponsiveLayout(Width);
        }

        // Método que adapta la interfaz dependiendo del ancho
        private void ApplyResponsiveLayout(double width)
        {
            // Si el ancho es inválido sale del método
            if (width <= 0) return;

            // Limpia columnas y filas anteriores
            ResponsiveGrid.ColumnDefinitions.Clear();
            ResponsiveGrid.RowDefinitions.Clear();

            // DISEÑO CELULAR
            if (width < 700)
            {
                // Agrega una columna
                ResponsiveGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Star });

                // Agrega filas automáticas
                ResponsiveGrid.RowDefinitions.Add(
                    new RowDefinition { Height = GridLength.Auto });

                ResponsiveGrid.RowDefinitions.Add(
                    new RowDefinition { Height = GridLength.Auto });

                // Coloca HeroPanel en fila 0
                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                // Coloca LoginPanel en fila 1
                Grid.SetColumn(LoginPanel, 0);
                Grid.SetRow(LoginPanel, 1);

                // Altura mínima hero
                HeroPanel.MinimumHeightRequest = 260;

                // Altura mínima login
                LoginPanel.MinimumHeightRequest = 520;

                // Ajusta ancho tarjeta login
                LoginCard.WidthRequest = -1;

                // Máximo ancho
                LoginCard.MaximumWidthRequest = 390;

                // Expande horizontalmente
                LoginCard.HorizontalOptions = LayoutOptions.Fill;

                // Padding del texto
                HeroText.Padding = new Thickness(26);
            }

            // DISEÑO TABLET
            else if (width < 1050)
            {
                // Una columna
                ResponsiveGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Star });

                // Dos filas
                ResponsiveGrid.RowDefinitions.Add(
                    new RowDefinition { Height = GridLength.Auto });

                ResponsiveGrid.RowDefinitions.Add(
                    new RowDefinition { Height = GridLength.Auto });

                // Hero arriba
                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                // Login abajo
                Grid.SetColumn(LoginPanel, 0);
                Grid.SetRow(LoginPanel, 1);

                // Alturas mínimas
                HeroPanel.MinimumHeightRequest = 320;
                LoginPanel.MinimumHeightRequest = 560;

                // Tamaños tarjeta
                LoginCard.WidthRequest = 420;
                LoginCard.MaximumWidthRequest = 440;

                // Centra tarjeta
                LoginCard.HorizontalOptions = LayoutOptions.Center;

                // Padding texto
                HeroText.Padding = new Thickness(40);
            }

            // DISEÑO ESCRITORIO
            else
            {
                // Una fila
                ResponsiveGrid.RowDefinitions.Add(
                    new RowDefinition { Height = GridLength.Star });

                // Dos columnas
                ResponsiveGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Star });

                ResponsiveGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Star });

                // Hero izquierda
                Grid.SetColumn(HeroPanel, 0);
                Grid.SetRow(HeroPanel, 0);

                // Login derecha
                Grid.SetColumn(LoginPanel, 1);
                Grid.SetRow(LoginPanel, 0);

                // Alturas
                HeroPanel.MinimumHeightRequest = 720;
                LoginPanel.MinimumHeightRequest = 720;

                // Tamaños tarjeta
                LoginCard.WidthRequest = 360;
                LoginCard.MaximumWidthRequest = 420;

                // Centrado
                LoginCard.HorizontalOptions = LayoutOptions.Center;

                // Padding texto
                HeroText.Padding = new Thickness(50);
            }
        }

        // Animación infinita de zoom en imagen
        private async Task AnimateHeroImage()
        {
            // Mientras las animaciones estén activas
            while (_runningAnimation)
            {
                // Hace zoom
                await HeroImage.ScaleTo(1.05, 1800, Easing.SinInOut);

                // Si se desactivó sale
                if (!_runningAnimation) break;

                // Regresa tamaño
                await HeroImage.ScaleTo(1.02, 1800, Easing.SinInOut);
            }
        }

        // Animación infinita de movimiento tarjeta
        private async Task AnimateLoginCard()
        {
            // Mientras siga activa
            while (_runningAnimation)
            {
                // Sube ligeramente
                await LoginCard.TranslateTo(0, -3, 1200, Easing.SinInOut);

                // Verifica si sigue activa
                if (!_runningAnimation) break;

                // Regresa posición
                await LoginCard.TranslateTo(0, 0, 1200, Easing.SinInOut);
            }
        }

        // Evento cuando un Entry obtiene foco
        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            // Verifica que sea Entry
            if (sender is Entry entry)

                // Agranda ligeramente
                await entry.ScaleTo(1.01, 80, Easing.CubicOut);
        }

        // Evento cuando Entry pierde foco
        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            // Verifica tipo Entry
            if (sender is Entry entry)

                // Regresa tamaño normal
                await entry.ScaleTo(1.0, 80, Easing.CubicOut);
        }

        // Genera captcha aleatorio
        private void GenerateCaptcha()
        {
            // Generador aleatorio
            Random rnd = new Random();

            // Número 1
            int val1 = rnd.Next(1, 10);

            // Número 2
            int val2 = rnd.Next(1, 10);

            // Guarda resultado correcto
            _captchaResult = val1 + val2;

            // Muestra operación
            CaptchaLabel.Text = $"{val1} + {val2} =";

            // Limpia entrada captcha
            CaptchaEntry.Text = string.Empty;
        }

        // Botón refrescar captcha
        private void OnRefreshCaptchaClicked(object sender, EventArgs e)
        {
            // Genera nuevo captcha
            GenerateCaptcha();
        }

        // Método para mostrar mensajes
        private void ShowMessage(string text)
        {
            // Cambia texto del mensaje
            MessageLabel.Text = text;

            // Hace visible el label
            MessageLabel.IsVisible = true;
        }

        // Evento al presionar botón login
        private async void OnButtonPressed(object sender, EventArgs e)
        {
            // Hace pequeño el botón
            await LoginButton.ScaleTo(0.96, 70, Easing.CubicOut);
        }

        // Evento al soltar botón
        private async void OnButtonReleased(object sender, EventArgs e)
        {
            // Regresa tamaño normal
            await LoginButton.ScaleTo(1, 90, Easing.CubicOut);
        }

        // Evento principal login
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Obtiene usuario
            string user = UsernameEntry.Text?.Trim() ?? string.Empty;

            // Obtiene contraseña
            string pass = PasswordEntry.Text ?? string.Empty;

            // Obtiene captcha
            string captchaInput = CaptchaEntry.Text?.Trim() ?? string.Empty;

            // Verifica campos vacíos
            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                // Muestra error
                ShowMessage("Por favor, ingresa tus credenciales.");
                return;
            }

            // Verifica captcha
            if (captchaInput != _captchaResult.ToString())
            {
                // Error captcha
                ShowMessage("La suma es incorrecta.");

                // Nuevo captcha
                GenerateCaptcha();
                return;
            }

            // Verifica si es admin
            if (user.Equals(AdminUser,
                StringComparison.OrdinalIgnoreCase)
                && pass == AdminPass)
            {
                // Marca admin
                AppData.IsAdmin = true;

                // Abre AppShell principal
                Application.Current!.Windows[0].Page = new AppShell();

                return;
            }

            // Login con servicio
            bool ok = await AuthService.LoginAsync(user, pass);

            // Si login correcto
            if (ok)
            {
                // Obtiene rol usuario
                string rol =
                    await AuthService.GetCurrentUserRoleAsync();

                // No es admin
                AppData.IsAdmin = false;

                // Si es repartidor
                if (rol == "Repartidor")
                {
                    // Busca perfil del repartidor
                    var perfil =
                        await RepartidorService.ObtenerAsync(user);

                    // Si NO tiene perfil registrado
                    if (perfil == null)
                    {
                        // Lo manda al registro
                        Application.Current!.Windows[0].Page =
                            new NavigationPage(
                                new RepartidorRegistroPage());

                        return;
                    }

                    // Si ya tiene perfil
                    Application.Current!.Windows[0].Page =
                        new NavigationPage(
                            new RepartidorPage());

                    return;
                }

                // Si es vendedor
                if (rol == "Vendedor")
                {
                    // Busca tienda registrada
                    var tienda =
                        await SellerStoreService.GetByOwnerAsync(user);

                    // Si NO tiene tienda
                    if (tienda == null)
                    {
                        // Lo manda a registrar tienda
                        Application.Current!.Windows[0].Page =
                            new NavigationPage(
                                new VendedorRegistroPage());

                        return;
                    }

                    // Si ya tiene tienda
                    Application.Current!.Windows[0].Page =
                        new NavigationPage(
                            new VendedorDashboardPage());

                    return;
                }

                // Usuario normal entra a AppShell
                Application.Current!.Windows[0].Page =
                    new AppShell();
            }
            else
            {
                // Error login
                ShowMessage("Usuario o contraseña incorrectos.");

                // Nuevo captcha
                GenerateCaptcha();
            }
        }

        // Evento al tocar "Registrarse"
        private async void OnRegisterTapped(
            object sender,
            TappedEventArgs e)
        {
            // Navega hacia RegisterPage
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
