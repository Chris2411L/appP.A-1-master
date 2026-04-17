namespace appP.A
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            // Create simple placeholders for expected product images (non-blocking)
            try
            {
                var filenames = new List<string>();
                var allProducts = new List<Models.Producto>();
                foreach (var cat in Models.AppData.Categorias)
                {
                    foreach (var p in cat.Productos)
                    {
                        if (!string.IsNullOrEmpty(p.Imagen)) filenames.Add(p.Imagen);
                        allProducts.Add(p);
                    }
                }
                // ensure placeholders exist
                System.Threading.Tasks.Task.Run(() => Services.ImageHelper.EnsurePlaceholders(filenames));
                // download remote images and cache them locally (so MAUI can load them reliably)
                System.Threading.Tasks.Task.Run(async () => await Services.ImageHelper.DownloadAndCacheRemoteImagesAsync(allProducts));
            }
            catch { }

        // Register image converter for fallback (so HTTP URLs and local files both work)
        // Also register in code in case XAML doesn't pick up (no harm)
        try { Resources.Add("ImageFallbackConverter", new Converters.ImageFallbackConverter()); } catch { }

        // Mostrar LoginPage al iniciar
        MainPage = new Microsoft.Maui.Controls.NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Microsoft.Maui.Graphics.Colors.White,
            BarTextColor = Microsoft.Maui.Graphics.Colors.Black
        };

        // Iniciar simulador de entregas en segundo plano (actualiza cada 15 segundos)
        try
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var todas = await Services.OrdenesService.ObtenerTodasOrdenesAsync();
                        foreach (var o in todas.Where(x => x.Status != "Entregado"))
                        {
                            // Avanzar estado según distancia al destino
                            double dx = o.DestLon - o.CurrentLon;
                            double dy = o.DestLat - o.CurrentLat;
                            // mover 10% del trayecto por tick
                            o.CurrentLon += dx * 0.1;
                            o.CurrentLat += dy * 0.1;

                            // actualizar status
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist < 0.0003) // cerca: entregado
                            {
                                o.Status = "Entregado";
                                var hist = System.Text.Json.JsonSerializer.Deserialize<List<string>>(o.HistoryJson) ?? new List<string>();
                                hist.Add($"{DateTime.Now:g}: Pedido entregado");
                                o.HistoryJson = System.Text.Json.JsonSerializer.Serialize(hist);
                                o.EstimatedDelivery = DateTime.Now;
                            }
                            else if (dist < 0.001)
                            {
                                o.Status = "En reparto";
                                var hist = System.Text.Json.JsonSerializer.Deserialize<List<string>>(o.HistoryJson) ?? new List<string>();
                                hist.Add($"{DateTime.Now:g}: Repartidor cerca");
                                o.HistoryJson = System.Text.Json.JsonSerializer.Serialize(hist);
                            }
                            else
                            {
                                o.Status = "En camino";
                            }

                            await Services.OrdenesService.ActualizarOrdenAsync(o);
                        }
                    }
                    catch { }
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(15));
                }
            });
        }
        catch { }
        }
    }
}