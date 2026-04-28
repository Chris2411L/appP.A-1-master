using appP.A.Models;

namespace appP.A
{
    public partial class ProductosPage : ContentPage
    {
        private readonly Categoria _categoria;
        private List<Producto> _productosBase = new();

        public ProductosPage(Categoria categoria)
        {
            InitializeComponent();

            _categoria = categoria;
            Title = categoria.Nombre;

            CategoriaTitle.Text = categoria.Nombre;
            CategoriaIcon.Text = string.IsNullOrWhiteSpace(categoria.Imagen) ? "🛒" : categoria.Imagen;

            _productosBase = categoria.Productos.ToList();
            ProductosList.ItemsSource = _productosBase;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            this.Opacity = 0;
            this.TranslationY = 20;

            await Task.WhenAll(
                this.FadeTo(1, 360, Easing.CubicOut),
                this.TranslateTo(0, 0, 360, Easing.CubicOut)
            );
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = e.NewTextValue?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(texto))
            {
                ProductosList.ItemsSource = _productosBase;
                return;
            }

            ProductosList.ItemsSource = _productosBase
                .Where(p =>
                    p.Nombre.ToLower().Contains(texto) ||
                    p.Descripcion.ToLower().Contains(texto))
                .ToList();
        }

        private async void OnAgregarAlCarritoClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Producto producto)
                return;

            if (producto.Stock <= 0)
            {
                await DisplayAlert("Sin stock", "Este producto no está disponible por ahora.", "OK");
                return;
            }

            AppData.CarritoActual.AgregarProducto(producto);

            await button.ScaleTo(0.94, 80, Easing.CubicOut);
            button.Text = "Agregado ✓";
            await button.ScaleTo(1, 120, Easing.CubicOut);

            await Task.Delay(650);

            if (producto.Stock > 0)
                button.Text = "Añadir al carrito";
        }

        private async void OnVerCarritoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CarritoPage());
        }
    }
}