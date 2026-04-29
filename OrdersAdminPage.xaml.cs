using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class OrdersAdminPage : ContentPage
    {
        public OrdersAdminPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            OrdersRefreshView.IsRefreshing = true;
            OrdersCollection.Opacity = 0;
            OrdersCollection.TranslationY = 18;

            OrdersCollection.ItemsSource = await OrdenesService.ObtenerTodasOrdenesAsync();

            OrdersRefreshView.IsRefreshing = false;

            await Task.WhenAll(
                OrdersCollection.FadeTo(1, 350, Easing.CubicOut),
                OrdersCollection.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
        }

        private void OnRefreshing(object sender, EventArgs e)
        {
            OnAppearing();
        }

        private async void OnOrderSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Orden o)
            {
                ((CollectionView)sender).SelectedItem = null;

                string ticket =
                    $"ID: {o.Id}\n" +
                    $"Usuario: {o.Usuario}\n" +
                    $"Fecha: {o.Fecha:g}\n" +
                    $"Dirección: {o.Direccion}\n" +
                    $"Pago: {o.MetodoPago}\n" +
                    $"Estado: {o.Status}\n\n" +
                    $"Detalles:\n{o.Detalles.Replace(", ", "\n")}\n\n" +
                    $"TOTAL: {o.Total:C2}";

                await DisplayAlert("Detalles de Orden", ticket, "OK");
            }
        }
    }
}