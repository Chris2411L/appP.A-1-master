using appP.A.Models;
using appP.A.Services;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace appP.A
{
    public partial class DashboardPage : ContentPage
    {
        private List<ChartPoint> _ventasSemana = new();

        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            DashboardContent.Opacity = 0;
            DashboardContent.TranslationY = 18;

            if (AppData.IsAdmin)
                await CargarDashboardAdmin();
            else
                await CargarDashboardCliente();

            await Task.WhenAll(
                DashboardContent.FadeTo(1, 380, Easing.CubicOut),
                DashboardContent.TranslateTo(0, 0, 380, Easing.CubicOut)
            );
        }

        private async Task CargarDashboardAdmin()
        {
            AdminPanel.IsVisible = true;
            ClientPanel.IsVisible = false;

            DashboardTitleLabel.Text = "Dashboard Admin";
            DashboardSubtitleLabel.Text = "Ventas, pedidos y productos más vendidos.";

            var ordenes = await OrdenesService.ObtenerTodasOrdenesAsync();

            double ventasTotales = ordenes.Sum(o => o.Total);
            int pedidosHoy = ordenes.Count(o => o.Fecha.Date == DateTime.Today);
            double ticketPromedio = ordenes.Count == 0 ? 0 : ventasTotales / ordenes.Count;

            VentasTotalesLabel.Text = ventasTotales.ToString("C2", CultureInfo.CurrentCulture);
            PedidosHoyLabel.Text = pedidosHoy.ToString();
            TicketPromedioLabel.Text = ticketPromedio.ToString("C2", CultureInfo.CurrentCulture);

            var productos = ObtenerProductosVendidos(ordenes);

            ProductoTopLabel.Text = productos.Count > 0
                ? productos[0].Nombre
                : "Sin datos";

            ProductosVendidosList.ItemsSource = productos.Take(8).ToList();

            _ventasSemana = ObtenerVentasSemana(ordenes);
            SalesChartView.Drawable = new SalesChartDrawable(_ventasSemana);
            SalesChartView.Invalidate();
        }

        private async Task CargarDashboardCliente()
        {
            AdminPanel.IsVisible = false;
            ClientPanel.IsVisible = true;

            DashboardTitleLabel.Text = "Mi Dashboard";
            DashboardSubtitleLabel.Text = "Resumen personal de tus compras.";

            var usuario = AuthService.GetCurrentUser() ?? "invitado";
            var ordenes = await OrdenesService.ObtenerOrdenesUsuarioAsync(usuario);

            MisPedidosLabel.Text = ordenes.Count.ToString();
            MiGastoLabel.Text = ordenes.Sum(o => o.Total).ToString("C2", CultureInfo.CurrentCulture);

            RecentOrdersList.ItemsSource = ordenes
                .OrderByDescending(o => o.Fecha)
                .Take(5)
                .ToList();
        }

        private List<ProductoVendido> ObtenerProductosVendidos(List<Orden> ordenes)
        {
            var contador = new Dictionary<string, int>();

            foreach (var orden in ordenes)
            {
                if (string.IsNullOrWhiteSpace(orden.Detalles))
                    continue;

                var partes = orden.Detalles.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var parte in partes)
                {
                    var texto = parte.Trim();

                    int cantidad = 1;
                    string nombre = texto;

                    int xIndex = texto.IndexOf('x');

                    if (xIndex > 0)
                    {
                        string cantidadTexto = texto.Substring(0, xIndex).Trim();

                        if (int.TryParse(cantidadTexto, out int cantidadParseada))
                            cantidad = cantidadParseada;

                        nombre = texto.Substring(xIndex + 1).Trim();
                    }

                    if (string.IsNullOrWhiteSpace(nombre))
                        continue;

                    if (!contador.ContainsKey(nombre))
                        contador[nombre] = 0;

                    contador[nombre] += cantidad;
                }
            }

            return contador
                .OrderByDescending(x => x.Value)
                .Select(x => new ProductoVendido
                {
                    Nombre = x.Key,
                    Cantidad = x.Value
                })
                .ToList();
        }

        private List<ChartPoint> ObtenerVentasSemana(List<Orden> ordenes)
        {
            var hoy = DateTime.Today;
            var lista = new List<ChartPoint>();

            for (int i = 6; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);

                lista.Add(new ChartPoint
                {
                    Label = fecha.ToString("ddd", new CultureInfo("es-MX")),
                    Value = ordenes
                        .Where(o => o.Fecha.Date == fecha)
                        .Sum(o => o.Total)
                });
            }

            return lista;
        }

        // IMPORTANTE:
        // Esta clase se mantiene para que no fallen tus controles antiguos:
        // BarsDrawable.cs, PieDrawable.cs y RadialBarsDrawable.cs
        public class ChartItem
        {
            public string Label { get; set; } = string.Empty;
            public double Valor { get; set; }
            public double Width { get; set; }
            public string PorcentajeTexto { get; set; } = string.Empty;
            public Color Color { get; set; } = Colors.Gray;
        }

        public class ProductoVendido
        {
            public string Nombre { get; set; } = "";
            public int Cantidad { get; set; }
        }

        public class ChartPoint
        {
            public string Label { get; set; } = "";
            public double Value { get; set; }
        }

        public class SalesChartDrawable : IDrawable
        {
            private readonly List<ChartPoint> _items;

            public SalesChartDrawable(List<ChartPoint> items)
            {
                _items = items;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                canvas.FillColor = Color.FromArgb("#111111");
                canvas.FillRectangle(dirtyRect);

                if (_items == null || _items.Count == 0)
                    return;

                float padding = 24;
                float chartHeight = dirtyRect.Height - 60;
                float barWidth = (dirtyRect.Width - padding * 2) / _items.Count - 10;

                double max = _items.Max(i => i.Value);
                if (max <= 0) max = 1;

                canvas.FontSize = 12;
                canvas.FontColor = Colors.White;

                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];

                    float x = padding + i * (barWidth + 10);
                    float h = (float)((item.Value / max) * chartHeight);
                    float y = dirtyRect.Height - 35 - h;

                    canvas.FillColor = Colors.White;
                    canvas.FillRoundedRectangle(x, y, barWidth, h, 8);

                    canvas.FontColor = Color.FromArgb("#D1D1D6");
                    canvas.DrawString(
                        item.Label,
                        x,
                        dirtyRect.Height - 26,
                        barWidth,
                        20,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center
                    );

                    if (item.Value > 0)
                    {
                        canvas.FontColor = Colors.White;
                        canvas.DrawString(
                            item.Value.ToString("C0"),
                            x - 8,
                            y - 22,
                            barWidth + 16,
                            18,
                            HorizontalAlignment.Center,
                            VerticalAlignment.Center
                        );
                    }
                }
            }
        }
    }
}