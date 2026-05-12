using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class RepartidorRutaPage : ContentPage
    {
        private readonly Orden _orden;
        private readonly bool _simular;

        public RepartidorRutaPage(Orden orden, bool simular = false)
        {
            InitializeComponent();
            _orden = orden;
            _simular = simular;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            double riderLat = _orden.CurrentLat == 0 ? _orden.BranchLat + 0.015 : _orden.CurrentLat;
            double riderLon = _orden.CurrentLon == 0 ? _orden.BranchLon - 0.015 : _orden.CurrentLon;

            RutaWebView.Navigating += RutaWebView_Navigating;

            RutaWebView.Source = new HtmlWebViewSource
            {
                Html = MapHtmlService.BuildRepartidorSimulationHtml(
                    riderLat,
                    riderLon,
                    _orden.BranchLat,
                    _orden.BranchLon,
                    _orden.DestLat,
                    _orden.DestLon
                )
            };

            InfoRutaLabel.Text = _simular
                ? "Simulando entrega en tiempo real..."
                : "Ruta del pedido";
        }

        private async void RutaWebView_Navigating(object? sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("nontonio://delivered"))
                return;

            e.Cancel = true;

            _orden.Status = "Entregado";
            _orden.HistoryJson += $"{DateTime.Now:o}|Entregado;";

            try
            {
                await OrdenesService.ActualizarOrdenAsync(_orden);
            }
            catch
            {
                await OrdenesService.GuardarOrdenAsync(_orden);
            }

            await DisplayAlert("Entrega finalizada", "El pedido fue entregado correctamente.", "OK");
            await Navigation.PopAsync();
        }
    }
}