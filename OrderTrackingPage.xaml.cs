using appP.A.Models;
using appP.A.Services;
using System.Globalization;
using System.Text.Json;

namespace appP.A;

public partial class OrderTrackingPage : ContentPage
{
    private readonly int _orderId;
    private bool _running;

    public OrderTrackingPage(int orderId)
    {
        InitializeComponent();
        _orderId = orderId;
        _running = true;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var o = await OrdenesService.ObtenerOrdenPorIdAsync(_orderId);
            if (o == null)
            {
                await DisplayAlert("Error", "Pedido no encontrado.", "OK");
                await Navigation.PopAsync();
                return;
            }

            StatusLabel.Text = FormatStatus(o);

            // Load initial map using Leaflet HTML that includes branch, destination and courier icons
            // If branch info is missing, fall back to using destination as branch
            double bLat = double.IsNaN(o.BranchLat) || o.BranchLat == 0 ? o.DestLat : o.BranchLat;
            double bLon = double.IsNaN(o.BranchLon) || o.BranchLon == 0 ? o.DestLon : o.BranchLon;
            var html = MapHtmlService.BuildLeafletHtml(o.DestLat, o.DestLon, o.CurrentLat, o.CurrentLon, bLat, bLon);
            MapView.Source = new HtmlWebViewSource { Html = html };

            // small delay to allow WebView to load before sending JS updates
            await Task.Delay(800);

            _ = StartRefreshLoop();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private string FormatStatus(Orden o)
    {
        return o.Status + (o.EstimatedDelivery.HasValue ? $" • ETA: {o.EstimatedDelivery:HH:mm}" : string.Empty);
    }

    private async Task StartRefreshLoop()
    {
        while (_running)
        {
            try
            {
                var o = await OrdenesService.ObtenerOrdenPorIdAsync(_orderId);
                if (o != null)
                {
                    // update status and history on UI thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusLabel.Text = FormatStatus(o);
                        try
                        {
                            var hist = JsonSerializer.Deserialize<List<string>>(o.HistoryJson) ?? new List<string>();
                            HistoryList.ItemsSource = hist.Reverse<string>().ToList();
                        }
                        catch { }
                    });

                    // update courier marker on the map via JS
                    if (!double.IsNaN(o.CurrentLat) && !double.IsNaN(o.CurrentLon))
                    {
                        try
                        {
                            var js = $"updateCourier({o.CurrentLat.ToString(CultureInfo.InvariantCulture)},{o.CurrentLon.ToString(CultureInfo.InvariantCulture)});";
                            await MapView.EvaluateJavaScriptAsync(js);

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                MapFallbackImage.IsVisible = false;
                                MapErrorLabel.IsVisible = false;
                            });
                        }
                        catch
                        {
                            // if JS fails, fallback to static image centered on destination
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                try
                                {
                                    var staticUrl = $"https://staticmap.openstreetmap.de/staticmap.php?center={o.DestLat.ToString(CultureInfo.InvariantCulture)},{o.DestLon.ToString(CultureInfo.InvariantCulture)}&zoom=15&size=800x360&markers={o.DestLat.ToString(CultureInfo.InvariantCulture)},{o.DestLon.ToString(CultureInfo.InvariantCulture)},red-pushpin";
                                    MapFallbackImage.Source = ImageSource.FromUri(new Uri(staticUrl));
                                    MapFallbackImage.IsVisible = true;
                                }
                                catch { }
                            });
                        }
                    }
                }
            }
            catch { }

            await Task.Delay(3000);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _running = false;
    }

    private void MapView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        MapErrorLabel.IsVisible = e.Result != WebNavigationResult.Success;
        if (e.Result != WebNavigationResult.Success)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var o = await OrdenesService.ObtenerOrdenPorIdAsync(_orderId);
                    if (o != null)
                    {
                        var staticUrl = $"https://staticmap.openstreetmap.de/staticmap.php?center={o.DestLat.ToString(CultureInfo.InvariantCulture)},{o.DestLon.ToString(CultureInfo.InvariantCulture)}&zoom=15&size=800x360&markers={o.DestLat.ToString(CultureInfo.InvariantCulture)},{o.DestLon.ToString(CultureInfo.InvariantCulture)},red-pushpin";
                        MapFallbackImage.Source = ImageSource.FromUri(new Uri(staticUrl));
                        MapFallbackImage.IsVisible = true;
                    }
                }
                catch { }
            });
        }
    }
}