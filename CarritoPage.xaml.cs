using appP.A.Models;

namespace appP.A;

public partial class CarritoPage : ContentPage
{
    public CarritoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        RefreshCarrito();
    }

    // =========================================
    // REFRESCAR
    // =========================================

    private void RefreshCarrito()
    {
        CartItemsView.ItemsSource = null;

        CartItemsView.ItemsSource =
            AppData.CarritoActual.Productos;

        EmptyState.IsVisible =
            !AppData.CarritoActual.Productos.Any();

        TotalLabel.Text =
            AppData.CarritoActual.Total
            .ToString("C2");
    }

    // =========================================
    // SUMAR
    // =========================================

    private void OnAddClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button btn &&
            btn.CommandParameter is Producto p)
        {
            p.Cantidad++;

            RefreshCarrito();
        }
    }

    // =========================================
    // RESTAR
    // =========================================

    private void OnRemoveClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button btn &&
            btn.CommandParameter is Producto p)
        {
            AppData.CarritoActual
                .ReducirCantidad(p);

            RefreshCarrito();
        }
    }

    // =========================================
    // ELIMINAR
    // =========================================

    private void OnDeleteClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button btn &&
            btn.CommandParameter is Producto p)
        {
            AppData.CarritoActual
                .EliminarProductoDirecto(p);

            RefreshCarrito();
        }
    }

    // =========================================
    // VACIAR
    // =========================================

    private void OnClearCartClicked(
        object sender,
        EventArgs e)
    {
        AppData.CarritoActual
            .Vaciar();

        RefreshCarrito();
    }

    // =========================================
    // CHECKOUT
    // =========================================

    private async void OnCheckoutClicked(
        object sender,
        EventArgs e)
    {
        if (!AppData.CarritoActual
                .Productos
                .Any())
        {
            await DisplayAlert(
                "Carrito vacío",
                "Agrega productos primero.",
                "OK");

            return;
        }

        await Navigation.PushAsync(
            new CheckoutPage());
    }

    // =========================================
    // IR A COMPRAR
    // =========================================

    private async void OnGoShoppingClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new CategoriasPage());
    }
}