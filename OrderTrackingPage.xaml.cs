using appP.A.Models;
using appP.A.Services;
using System.Globalization;
using System.Collections.ObjectModel;
using Microsoft.Maui.Dispatching;

namespace appP.A;

public partial class OrderTrackingPage : ContentPage
{
    private readonly int _orderId;
    private bool _running = true;
    private ObservableCollection<string> _history = new();

    // simple token to avoid overlapping simulations
    private bool _simulationRunning = false;

    public OrderTrackingPage(int orderId)
    {
        InitializeComponent();
        _orderId = orderId;
        HistoryList.ItemsSource = _history;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarSeguimientoRealista();
    }

    private async Task CargarSeguimientoRealista()
    {
        var orden = await OrdenesService.ObtenerOrdenPorIdAsync(_orderId);
        if (orden == null) return;

        StatusLabel.Text = $"Enviando a: {orden.Direccion}";

        // inicializar historial con el estado actual
        _history.Clear();
        _history.Add($"{DateTime.Now:HH:mm} — Estado: {orden.Status}");

        // 1. Convertir la dirección escrita en coordenadas reales para el logo de la casa
        if (orden.DestLat == 0 || orden.DestLon == 0)
        {
            try
            {
                var locations = await Geocoding.Default.GetLocationsAsync(orden.Direccion);
                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    orden.DestLat = location.Latitude;
                    orden.DestLon = location.Longitude;
                }
                else
                {
                    // Ubicación por defecto en México si falla el geocoding
                    orden.DestLat = 19.4326; orden.DestLon = -99.1332;
                }
            }
            catch { orden.DestLat = 19.4326; orden.DestLon = -99.1332; }
        }

        // 2. Buscar la sucursal más cercana en el país
        var sucursal = SucursalesService.ObtenerMasCercana(orden.DestLat, orden.DestLon);

        // 3. Generar el mapa con la ruta por carreteras
        var html = MapHtmlService.BuildLeafletHtml(orden.DestLat, orden.DestLon, sucursal.Lat, sucursal.Lon);
        MapView.Source = new HtmlWebViewSource { Html = html };

        _ = IniciarRelojETA();

        // iniciar simulación de actualizaciones en tiempo real (local)
        if (!_simulationRunning)
        {
            _simulationRunning = true;
            _ = SimularActualizacionesDeEstadoAsync(orden);
        }
    }

    private async Task SimularActualizacionesDeEstadoAsync(Orden orden)
    {
        try
        {
            // secuencia de estados realistas
            var pasos = new[] { "Preparando", "Listo para salir", "En camino", "En reparto", "Entregado" };
            // empezar desde el estado actual
            var current = Array.IndexOf(pasos, orden.Status);
            if (current < 0) current = 0;

            for (int i = current + 1; i < pasos.Length && _running; i++)
            {
                // esperar entre 20 y 50 segundos para la simulación (más realista)
                var wait = 20 + (i * 10);
                for (int s = 0; s < wait && _running; s++) await Task.Delay(1000);

                orden.Status = pasos[i];
                orden.HistoryJson += $"{DateTime.Now:o}|{orden.Status};";

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusLabel.Text = orden.Status;
                    var texto = $"{DateTime.Now:HH:mm} — Estado: {orden.Status}";
                    _history.Add(texto);
                    ShowNotification($"Estado actualizado: {orden.Status}");

                    if (orden.Status == "Entregado")
                    {
                        ETALabel.Text = "Pedido en tu puerta";
                        ConfirmDeliveryButton.IsVisible = true;
                    }
                });
            }
        }
        finally
        {
            _simulationRunning = false;
        }
    }

    void ShowNotification(string text)
    {
        NotificationText.Text = text;
        NotificationBanner.Opacity = 0;
        NotificationBanner.IsVisible = true;
        _ = NotificationBanner.FadeTo(1, 220);
        // auto hide
        _ = Task.Run(async () =>
        {
            await Task.Delay(3500);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await NotificationBanner.FadeTo(0, 220);
                NotificationBanner.IsVisible = false;
            });
        });
    }

    private void OnNotificationViewClicked(object sender, EventArgs e)
    {
        // scroll to history (simple focus)
        if (_history.Count > 0) HistoryList.ScrollTo(_history[^1]);
    }

    private async void OnConfirmDeliveryClicked(object sender, EventArgs e)
    {
        // marcar orden como recibida y mostrar agradecimiento
        await DisplayAlert("Entrega", "Gracias — entrega confirmada.", "OK");
        ConfirmDeliveryButton.IsVisible = false;
    }

    private async Task IniciarRelojETA()
    {
        int segundos = 180;
        while (segundos > 0 && _running)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                ETALabel.Text = $"Llegada en: {segundos / 60}:{segundos % 60:D2} min";
            });
            await Task.Delay(1000);
            segundos--;
        }
        if (_running)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                StatusLabel.Text = "¡Entregado!";
                ETALabel.Text = "Pedido en tu puerta";
            });
        }
    }

    public void MapView_Navigated(object sender, WebNavigatedEventArgs e) { }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _running = false;
    }
}