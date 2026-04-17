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
            OrdersCollection.ItemsSource = await OrdenesService.ObtenerTodasOrdenesAsync();
            OrdersRefreshView.IsRefreshing = false;
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
                string ticket = $"ID: {o.Id}\nUsuario: {o.Usuario}\nFecha: {o.Fecha:g}\nDirección: {o.Direccion}\nPago: {o.MetodoPago}\nDetalles:\n{o.Detalles.Replace(", ", "\n")}\n\nTOTAL: {o.Total:C2}";
                await DisplayAlert("Detalles de Orden", ticket, "OK");
            }
        }
    }
}