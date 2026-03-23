using appP.A.Models;
using System.Collections.Specialized;

namespace appP.A
{
    public partial class CarritoPage : ContentPage
    {
        public CarritoPage()
        {
            InitializeComponent();

            BindingContext = AppData.CarritoActual;
            UpdateEmptyState();

            if (AppData.CarritoActual?.Productos is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged += Productos_CollectionChanged;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateEmptyState();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (AppData.CarritoActual?.Productos is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged -= Productos_CollectionChanged;
            }
        }

        private void Productos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateEmptyState);
        }

        private void UpdateEmptyState()
        {
            var empty = AppData.CarritoActual == null || !AppData.CarritoActual.Productos.Any();
            EmptyState.IsVisible = empty;
            ItemsList.IsVisible = !empty;
        }

        // --- MÉTODOS PARA LOS BOTONES + Y - ---

        private void OnAumentarCantidadClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.CommandParameter as Producto;
            if (producto != null)
            {
                AppData.CarritoActual.AgregarProducto(producto, 1);
            }
        }

        private void OnReducirCantidadClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.CommandParameter as Producto;
            if (producto != null)
            {
                AppData.CarritoActual.ReducirCantidad(producto);
            }
        }

        private async void OnEliminarProductoClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var producto = btn?.CommandParameter as Producto;
            if (producto != null)
            {
                bool answer = await DisplayAlert("Quitar", $"¿Deseas quitar '{producto.Nombre}' del carrito?", "Sí", "No");
                if (answer)
                {
                    AppData.CarritoActual.EliminarProductoDirecto(producto);
                }
            }
        }

        // ----------------------------------------------

        private async void OnVaciarClicked(object sender, EventArgs e)
        {
            if (AppData.CarritoActual == null || !AppData.CarritoActual.Productos.Any()) return;

            var ok = await DisplayAlert("Confirmar", "¿Quieres vaciar todo el carrito?", "Sí", "No");
            if (!ok) return;

            AppData.CarritoActual?.Vaciar();
            UpdateEmptyState();
        }

        // ESTA ES LA MAGIA QUE CONECTA EL CARRITO CON EL CHECKOUT
        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (AppData.CarritoActual == null || !AppData.CarritoActual.Productos.Any())
            {
                await DisplayAlert("Aviso", "El carrito está vacío.", "OK");
                return;
            }

            // En lugar de mostrar la alerta final, navegamos a la pantalla de Checkout
            await Navigation.PushAsync(new CheckoutPage());
        }

        private async void OnSeguirComprandoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}