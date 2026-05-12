using appP.A.Models;
using appP.A.Services;

namespace appP.A;

public partial class VendedorDashboardPage : ContentPage
{
    SellerStore? _store;

    public VendedorDashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var user = AuthService.GetCurrentUser();

        if (string.IsNullOrWhiteSpace(user))
            return;

        _store = await SellerStoreService.GetByOwnerAsync(user);

        if (_store == null)
        {
            Application.Current!.Windows[0].Page =
                new NavigationPage(new VendedorRegistroPage());

            return;
        }

        StoreNameLabel.Text = _store.StoreName;
        StoreAddressLabel.Text = _store.Address;

        EarningsLabel.Text =
            _store.Earnings.ToString("C");

        OrdersLabel.Text =
            _store.TotalOrders.ToString();
    }

    private async void OnInventoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AlmacenAdminPage());
    }

    private async void OnOrdersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrdersAdminPage());
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        AuthService.Logout();

        Application.Current!.Windows[0].Page =
            new NavigationPage(new LoginPage());
    }
}