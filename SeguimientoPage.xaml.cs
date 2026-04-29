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

            TrackingOrdersList.Opacity = 0;
            TrackingOrdersList.TranslationY = 18;

            TrackingOrdersList.ItemsSource = await OrdenesService.ObtenerTodasOrdenesAsync();

            await Task.WhenAll(
                TrackingOrdersList.FadeTo(1, 350, Easing.CubicOut),
                TrackingOrdersList.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
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