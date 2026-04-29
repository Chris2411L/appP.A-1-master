using appP.A.Models;
using appP.A.Services;
using System.Collections.ObjectModel;

namespace appP.A
{
    public partial class AlmacenAdminPage : ContentPage
    {
        private ObservableCollection<Producto> _todosLosProductos;
        public ObservableCollection<Producto> ProductosFiltrados { get; set; }

        public AlmacenAdminPage()
        {
            InitializeComponent();

            _todosLosProductos = new ObservableCollection<Producto>();
            ProductosFiltrados = new ObservableCollection<Producto>();

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AsegurarDatos();
            CargarInventario();
        }

        private void AsegurarDatos()
        {
            if (AppData.Categorias == null)
                AppData.Categorias = new List<Categoria>();

            if (AppData.Categorias.Count == 0)
            {
                AppData.Categorias.Add(new Categoria("Bebidas", "🥤"));
                AppData.Categorias.Add(new Categoria("Snacks", "🍿"));
                AppData.Categorias.Add(new Categoria("Dulces", "🍬"));
                AppData.Categorias.Add(new Categoria("Chocolates", "🍫"));
                AppData.Categorias.Add(new Categoria("Galletas", "🍪"));
            }

            foreach (var categoria in AppData.Categorias)
            {
                if (categoria.Productos == null)
                    categoria.Productos = new List<Producto>();

                if (categoria.Productos.Count == 0)
                    LlenarProductosCategoria(categoria);
            }
        }

        private void LlenarProductosCategoria(Categoria categoria)
        {
            string[] productos = categoria.Nombre switch
            {
                "Bebidas" => new[] { "Cola", "Naranja", "Limón", "Agua Mineral", "Café Frío" },
                "Snacks" => new[] { "Sabritas", "Doritos", "Cheetos", "Takis", "Ruffles" },
                "Dulces" => new[] { "Paleta", "Panditas", "Caramelo", "Mazapán", "Skittles" },
                "Chocolates" => new[] { "Snickers", "KitKat", "Ferrero", "Kinder", "Crunch" },
                "Galletas" => new[] { "Oreo", "Chokis", "Marías", "Canelitas", "Príncipe" },
                _ => new[] { "Producto clásico", "Producto premium", "Producto especial" }
            };

            foreach (var nombre in productos)
            {
                categoria.Productos.Add(new Producto
                {
                    Id = AppData.GetNextProductId(),
                    Nombre = $"{categoria.Nombre} {nombre}",
                    Categoria = categoria.Nombre,
                    Descripcion = $"Producto de {categoria.Nombre}",
                    Precio = 20,
                    PrecioAnterior = 25,
                    Rating = 4.5,
                    Stock = 50,
                    Imagen = $"https://tse.mm.bing.net/th?q={Uri.EscapeDataString(nombre + " producto")}&w=800&h=800&c=7&rs=1&p=0&o=5&pid=1.7"
                });
            }
        }

        private void CargarInventario()
        {
            _todosLosProductos.Clear();

            foreach (var categoria in AppData.Categorias)
            {
                foreach (var producto in categoria.Productos)
                {
                    _todosLosProductos.Add(producto);
                }
            }

            var listaCategorias = AppData.Categorias
                .Select(c => c.Nombre)
                .Distinct()
                .ToList();

            listaCategorias.Insert(0, "Todas las categorías");

            CategoriaPicker.ItemsSource = listaCategorias;
            CategoriaPicker.SelectedIndex = 0;

            Filtrar();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrar();
        }

        private void OnCategoriaChanged(object sender, EventArgs e)
        {
            Filtrar();
        }

        private void Filtrar()
        {
            if (_todosLosProductos == null)
                return;

            string textoBusqueda = SearchBox.Text?.ToLower().Trim() ?? "";
            string categoriaSeleccionada = CategoriaPicker.SelectedItem as string ?? "Todas las categorías";

            ProductosFiltrados.Clear();

            var resultados = _todosLosProductos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(textoBusqueda))
                resultados = resultados.Where(p => p.Nombre.ToLower().Contains(textoBusqueda));

            if (categoriaSeleccionada != "Todas las categorías")
                resultados = resultados.Where(p => p.Categoria == categoriaSeleccionada);

            foreach (var producto in resultados)
                ProductosFiltrados.Add(producto);
        }

        private async void OnSumarStock(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                p.Stock++;
                await InventarioService.ActualizarStockAsync(p.Id, p.Stock);
                CargarInventario();
            }
        }

        private async void OnRestarStock(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                if (p.Stock > 0)
                {
                    p.Stock--;
                    await InventarioService.ActualizarStockAsync(p.Id, p.Stock);
                    CargarInventario();
                }
            }
        }

        private async void OnEditarStock(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                string result = await DisplayPromptAsync(
                    "Modificar Inventario",
                    $"Ingresa la cantidad para:\n{p.Nombre}",
                    initialValue: p.Stock.ToString(),
                    keyboard: Keyboard.Numeric);

                if (int.TryParse(result, out int nuevaCantidad) && nuevaCantidad >= 0)
                {
                    p.Stock = nuevaCantidad;
                    await InventarioService.ActualizarStockAsync(p.Id, p.Stock);
                    await ProductService.UpdateAsync(p);
                    CargarInventario();
                }
            }
        }

        private async void OnAgregarProductoClicked(object sender, EventArgs e)
        {
            string nombre = await DisplayPromptAsync("Nuevo producto", "Nombre:");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string categoria = await DisplayPromptAsync("Nuevo producto", "Categoría:");
            if (string.IsNullOrWhiteSpace(categoria)) categoria = "Sin categoría";

            string precioText = await DisplayPromptAsync("Nuevo producto", "Precio:", keyboard: Keyboard.Numeric);
            if (!double.TryParse(precioText, out double precio)) precio = 0;

            var prod = new Producto
            {
                Id = AppData.GetNextProductId(),
                Nombre = nombre.Trim(),
                Categoria = categoria.Trim(),
                Descripcion = $"Producto de {categoria}",
                Precio = precio,
                PrecioAnterior = precio + 5,
                Rating = 4.5,
                Stock = 50,
                Imagen = $"https://tse.mm.bing.net/th?q={Uri.EscapeDataString(nombre + " producto")}&w=800&h=800&c=7&rs=1&p=0&o=5&pid=1.7"
            };

            var admin = AuthService.GetCurrentUser() ?? "admin";

            try
            {
                int newId = await ProductService.AddAsync(prod, admin);
                prod.Id = newId;
            }
            catch { }

            var cat = AppData.Categorias.FirstOrDefault(c => c.Nombre == prod.Categoria);

            if (cat == null)
            {
                cat = new Categoria(prod.Categoria, "📦");
                AppData.Categorias.Add(cat);
            }

            cat.Productos.Add(prod);

            CargarInventario();

            await DisplayAlert("Éxito", "Producto agregado.", "OK");
        }

        private async void OnEliminarProductoClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                bool confirm = await DisplayAlert("Confirmar", $"¿Eliminar {p.Nombre}?", "Sí", "No");
                if (!confirm) return;

                var admin = AuthService.GetCurrentUser() ?? "admin";

                try
                {
                    await ProductService.DeleteAsync(p.Id, admin);
                }
                catch { }

                foreach (var c in AppData.Categorias)
                {
                    var ex = c.Productos.FirstOrDefault(x => x.Id == p.Id);
                    if (ex != null)
                    {
                        c.Productos.Remove(ex);
                        break;
                    }
                }

                CargarInventario();

                await DisplayAlert("Éxito", "Producto eliminado.", "OK");
            }
        }
    }
}