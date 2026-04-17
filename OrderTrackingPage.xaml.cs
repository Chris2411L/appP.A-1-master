using appP.A.Models;
using appP.A.Services;
using System.Text.Json;

namespace appP.A
{
    public partial class OrderTrackingPage : ContentPage
    {
        private int _orderId;
        private bool _running = true;

        public OrderTrackingPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _running = true;
            StartRefreshLoop();
        }

        protected override void OnDisappearing()
        {
            _running = false;
            base.OnDisappearing();
        }

        private async void StartRefreshLoop()
        {
            while (_running)
            {
                try
                {
                    var o = await OrdenesService.ObtenerOrdenPorIdAsync(_orderId);
                    if (o != null)
                    {
                        StatusLabel.Text = o.Status + (o.EstimatedDelivery.HasValue ? $" • ETA: {o.EstimatedDelivery:HH:mm}" : "");
                        var bbox = $"{(o.CurrentLon - 0.002).ToString(System.Globalization.CultureInfo.InvariantCulture)},{(o.CurrentLat - 0.002).ToString(System.Globalization.CultureInfo.InvariantCulture)},{(o.CurrentLon + 0.002).ToString(System.Globalization.CultureInfo.InvariantCulture)},{(o.CurrentLat + 0.002).ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                        var html = $"<html><body style='margin:0;padding:0'><iframe width='100%' height='100%' frameborder='0' src='https://www.openstreetmap.org/export/embed.html?bbox={bbox}&layer=mapnik&marker={o.CurrentLat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{o.CurrentLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}'></iframe></body></html>";
                        MapView.Source = new HtmlWebViewSource { Html = html };
                        var hist = JsonSerializer.Deserialize<List<string>>(o.HistoryJson) ?? new List<string>();
                        HistoryList.ItemsSource = hist.Reverse<string>().ToList();
                    }
                }
                catch { }
                await System.Threading.Tasks.Task.Delay(5000);
            }
        }
    }
}