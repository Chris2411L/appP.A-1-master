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
            CargarInventario();
        }

        private void CargarInventario()
        {
            _todosLosProductos.Clear();

            // 1. Extraer todos los productos de todas las categorías
            foreach (var categoria in AppData.Categorias)
            {
                foreach (var producto in categoria.Productos)
                {
                    _todosLosProductos.Add(producto);
                }
            }

            // 2. Cargar las categorías en el menú desplegable (Picker)
            var listaCategorias = AppData.Categorias.Select(c => c.Nombre).ToList();
            listaCategorias.Insert(0, "Todas las categorías"); // Opción para ver todo

            CategoriaPicker.ItemsSource = listaCategorias;
            CategoriaPicker.SelectedIndex = 0; // Esto disparará automáticamente la función Filtrar()
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            Filtrar();
        }

        private void OnCategoriaChanged(object sender, EventArgs e)
        {
            Filtrar();
        }

        // Esta función combina el filtro de texto y el de categoría
        private void Filtrar()
        {
            // Evitar errores si aún no se han cargado los componentes
            if (_todosLosProductos == null || !CategoriasCargadas()) return;

            string textoBusqueda = SearchBox.Text?.ToLower().Trim() ?? string.Empty;
            string categoriaSeleccionada = CategoriaPicker.SelectedItem as string ?? "Todas las categorías";

            ProductosFiltrados.Clear();

            // Partimos con la lista completa
            var resultados = _todosLosProductos.AsEnumerable();

            // 1. Filtrar por texto (si escribiste algo)
            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                resultados = resultados.Where(p => p.Nombre.ToLower().Contains(textoBusqueda));
            }

            // 2. Filtrar por categoría (si no está seleccionada la opción "Todas")
            if (categoriaSeleccionada != "Todas las categorías")
            {
                resultados = resultados.Where(p => p.Categoria == categoriaSeleccionada);
            }

            // Llenar la lista visual con los resultados filtrados
            foreach (var r in resultados)
            {
                ProductosFiltrados.Add(r);
            }
        }

        private bool CategoriasCargadas()
        {
            return CategoriaPicker.ItemsSource != null && CategoriaPicker.ItemsSource.Count > 0;
        }

        private async void OnSumarStock(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                p.Stock++;
                await InventarioService.ActualizarStockAsync(p.Id, p.Stock);
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
                }
            }
        }

        private async void OnEditarStock(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                string result = await DisplayPromptAsync("Modificar Inventario", $"Ingresa la cantidad exacta en almacén para:\n{p.Nombre}",
                                                         initialValue: p.Stock.ToString(), keyboard: Keyboard.Numeric);

                if (int.TryParse(result, out int nuevaCantidad) && nuevaCantidad >= 0)
                {
                    p.Stock = nuevaCantidad;
                    await InventarioService.ActualizarStockAsync(p.Id, p.Stock);
                    await ProductService.UpdateAsync(p);
                }
                else if (result != null)
                {
                    await DisplayAlert("Error", "Cantidad inválida.", "OK");
                }
            }
        }

        // Agregar producto nuevo desde el admin
        private async void OnAgregarProductoClicked(object sender, EventArgs e)
        {
            string nombre = await DisplayPromptAsync("Nuevo producto", "Nombre:");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string categoria = await DisplayPromptAsync("Nuevo producto", "Categoría:", initialValue: "");
            if (string.IsNullOrWhiteSpace(categoria)) categoria = "Sin categoría";

            string precioText = await DisplayPromptAsync("Nuevo producto", "Precio:", keyboard: Keyboard.Numeric);
            if (!double.TryParse(precioText, out double precio)) precio = 0.0;

            var prod = new Producto
            {
                Id = AppData.GetNextProductId(),
                Nombre = nombre.Trim(),
                Categoria = categoria.Trim(),
                Precio = precio,
                Stock = 50
            };

            // Guardar en DB
            var admin = AuthService.GetCurrentUser() ?? "admin";
            int newId = await ProductService.AddAsync(prod, admin);
            prod.Id = newId;

            // Añadir a AppData
            var cat = AppData.Categorias.FirstOrDefault(c => c.Nombre == prod.Categoria);
            if (cat == null)
            {
                cat = new Categoria(prod.Categoria, "");
                AppData.Categorias.Add(cat);
            }
            cat.Productos.Add(prod);

            // Recargar vista
            CargarInventario();
            await DisplayAlert("Éxito", "Producto agregado.", "OK");
        }

        private async void OnEliminarProductoClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                if (!await DisplayAlert("Confirmar", $"Eliminar {p.Nombre}?", "Sí", "No")) return;
                var admin = AuthService.GetCurrentUser() ?? "admin";
                bool ok = await ProductService.DeleteAsync(p.Id, admin);
                if (ok)
                {
                    // Eliminar de AppData
                    foreach (var c in AppData.Categorias)
                    {
                        var ex = c.Productos.FirstOrDefault(x => x.Id == p.Id);
                        if (ex != null) { c.Productos.Remove(ex); break; }
                    }
                    CargarInventario();
                    await DisplayAlert("Éxito", "Producto eliminado.", "OK");
                }
                else await DisplayAlert("Error", "No se pudo eliminar.", "OK");
            }
        }
    }
}