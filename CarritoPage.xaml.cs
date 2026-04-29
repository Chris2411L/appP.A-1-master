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

        private void Productos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateEmptyState);
        }

        private void UpdateEmptyState()
        {
            bool empty = AppData.CarritoActual == null || !AppData.CarritoActual.Productos.Any();

            EmptyState.IsVisible = empty;
            ItemsList.IsVisible = !empty;
            SummaryPanel.IsVisible = !empty;
        }

        private void OnAumentarCantidadClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
                AppData.CarritoActual.AgregarProducto(p, 1);
        }

        private void OnReducirCantidadClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
                AppData.CarritoActual.ReducirCantidad(p);
        }

        private async void OnEliminarProductoClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Producto p)
            {
                bool ok = await DisplayAlert("Eliminar", $"¿Quitar {p.Nombre}?", "Sí", "No");
                if (ok)
                    AppData.CarritoActual.EliminarProductoDirecto(p);
            }
        }

        private async void OnVaciarClicked(object sender, EventArgs e)
        {
            if (!AppData.CarritoActual.Productos.Any()) return;

            bool ok = await DisplayAlert("Vaciar", "¿Vaciar carrito?", "Sí", "No");
            if (ok)
                AppData.CarritoActual.Vaciar();
        }

        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (!AppData.CarritoActual.Productos.Any())
            {
                await DisplayAlert("Aviso", "Carrito vacío", "OK");
                return;
            }

            await Navigation.PushAsync(new CheckoutPage());
        }

        private async void OnSeguirComprandoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}