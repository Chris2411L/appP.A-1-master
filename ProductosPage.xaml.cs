using appP.A.Models;

namespace appP.A
{
    public partial class ProductosPage : ContentPage
    {
        public ProductosPage(Categoria categoria)
        {
            InitializeComponent();
            this.Title = categoria.Nombre;

            // CORRECCIÓN 2: Asignar directamente los productos de la categoría recibida
            ProductosList.ItemsSource = categoria.Productos.ToList();
        }

        private async void OnAgregarAlCarritoClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var producto = (Producto)button.CommandParameter;

            if (producto != null)
            {
                // CORRECCIÓN 1: Usar AgregarProducto() que es el nombre correcto en Carrito.cs
                AppData.CarritoActual.AgregarProducto(producto);
                await button.ScaleTo(1.1, 100);
                await button.ScaleTo(1.0, 100);
            }
        }

        private async void OnVerCarritoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CarritoPage());
        }
    }
}