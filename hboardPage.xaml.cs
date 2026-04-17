using appP.A.Models;
using appP.A.Services;
using System.Collections.ObjectModel;

namespace appP.A;

public partial class DashboardPage : ContentPage
{
    private const double MaxChartWidth = 200;

    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var usuario = AuthService.GetCurrentUser() ?? "invitado";
        var ordenes = await OrdenesService.ObtenerOrdenesUsuarioAsync(usuario);

        ViewModel.Cargar(ordenes);
    }

    public class DashboardViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public int TotalOrdenes { get; private set; }
        public double IngresosTotales { get; private set; }
        public double TicketPromedio { get; private set; }
        public int MetodosActivos { get; private set; }

        public ObservableCollection<ChartItem> VentasSemanales { get; } = new();
        public ObservableCollection<ChartItem> DistribucionPago { get; } = new();

        public void Cargar(List<Orden> ordenes)
        {
            TotalOrdenes = ordenes.Count;
            IngresosTotales = ordenes.Sum(x => x.Total);
            TicketPromedio = TotalOrdenes == 0 ? 0 : IngresosTotales / TotalOrdenes;
            MetodosActivos = ordenes.Select(x => x.MetodoPago).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count();

            OnPropertyChanged(nameof(TotalOrdenes));
            OnPropertyChanged(nameof(IngresosTotales));
            OnPropertyChanged(nameof(TicketPromedio));
            OnPropertyChanged(nameof(MetodosActivos));

            CargarVentasSemanales(ordenes);
            CargarDistribucionPago(ordenes);
        }

        private void CargarVentasSemanales(List<Orden> ordenes)
        {
            VentasSemanales.Clear();

            var hoy = DateTime.Today;
            var puntos = Enumerable.Range(0, 7)
                .Select(i => hoy.AddDays(-6 + i))
                .Select(fecha => new
                {
                    Fecha = fecha,
                    Total = ordenes.Where(o => o.Fecha.Date == fecha).Sum(o => o.Total)
                })
                .ToList();

            var max = puntos.Max(x => x.Total);
            if (max <= 0) max = 1;

            foreach (var p in puntos)
            {
                VentasSemanales.Add(new ChartItem
                {
                    Label = p.Fecha.ToString("ddd dd"),
                    Valor = p.Total,
                    Width = (p.Total / max) * MaxChartWidth,
                    Color = Color.FromArgb("#8A2BE2")
                });
            }
        }

        private void CargarDistribucionPago(List<Orden> ordenes)
        {
            DistribucionPago.Clear();

            var grupos = ordenes
                .Where(x => !string.IsNullOrWhiteSpace(x.MetodoPago))
                .GroupBy(x => x.MetodoPago)
                .Select(g => new { Metodo = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            var total = grupos.Sum(x => x.Cantidad);
            if (total == 0)
            {
                DistribucionPago.Add(new ChartItem
                {
                    Label = "Sin datos",
                    PorcentajeTexto = "0%",
                    Width = 0,
                    Color = Color.FromArgb("#9CA3AF")
                });
                return;
            }

            var palette = new[] { "#8A2BE2", "#10B981", "#F59E0B", "#EF4444", "#3B82F6" };

            for (int i = 0; i < grupos.Count; i++)
            {
                var p = (double)grupos[i].Cantidad / total;
                DistribucionPago.Add(new ChartItem
                {
                    Label = grupos[i].Metodo,
                    Valor = grupos[i].Cantidad,
                    PorcentajeTexto = $"{Math.Round(p * 100)}%",
                    Width = p * MaxChartWidth,
                    Color = Color.FromArgb(palette[i % palette.Length])
                });
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public double Valor { get; set; }
        public double Width { get; set; }
        public string PorcentajeTexto { get; set; } = string.Empty;
        public Color Color { get; set; } = Colors.Gray;
    }
}
