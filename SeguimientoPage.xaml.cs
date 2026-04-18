using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class SeguimientoPage : ContentPage
    {
        public SeguimientoPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // use existing service method name
            TrackingOrdersList.ItemsSource = await OrdenesService.ObtenerTodasOrdenesAsync();
        }

        private async void OnVerTrackingClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is Orden o)
            {
                await Navigation.PushAsync(new OrderTrackingPage(o.Id));
            }
        }

        private void OnTrackingOrderSelected(object sender, SelectionChangedEventArgs e)
        {
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
