namespace appP.A
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            // Mostrar LoginPage al iniciar
            MainPage = new Microsoft.Maui.Controls.NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Microsoft.Maui.Graphics.Colors.White,
                BarTextColor = Microsoft.Maui.Graphics.Colors.Black
            };
        }
    }
}