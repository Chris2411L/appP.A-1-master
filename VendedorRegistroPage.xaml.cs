using appP.A.Models;
using appP.A.Services;
using System.Globalization;

namespace appP.A;

public partial class VendedorRegistroPage : ContentPage
{
    double _lat = 19.4326;
    double _lon = -99.1332;

    string _image = "";

    public VendedorRegistroPage()
    {
        InitializeComponent();

        OpenPicker.SelectedIndex = 2;
        ClosePicker.SelectedIndex = 2;

        LoadMap();
    }

    void LoadMap()
    {
        string lat = _lat.ToString(CultureInfo.InvariantCulture);
        string lon = _lon.ToString(CultureInfo.InvariantCulture);

        string html = $@"
<!DOCTYPE html>
<html>
<head>

<meta name='viewport'
content='width=device-width, initial-scale=1.0'>

<link rel='stylesheet'
href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>

<style>

html,body,#map{{
height:100%;
margin:0;
padding:0;
}}

</style>

</head>

<body>

<div id='map'></div>

<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

<script>

var map = L.map('map').setView([{lat},{lon}],15);

L.tileLayer(
'https://tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png',
{{
maxZoom:19
}}
).addTo(map);

var icon = L.icon({{
iconUrl:'https://img.icons8.com/fluency/48/shop.png',
iconSize:[42,42],
iconAnchor:[21,42]
}});

var marker = L.marker(
[{lat},{lon}],
{{
draggable:true,
icon:icon
}}
).addTo(map);

function send(lat,lon)
{{
window.location.href =
'nontonio://location?lat='
+ lat +
'&lon=' +
lon;
}}

map.on('click', function(e)
{{
marker.setLatLng(e.latlng);

send(
e.latlng.lat,
e.latlng.lng
);
}});

marker.on('dragend', function(e)
{{
var p = marker.getLatLng();

send(
p.lat,
p.lng
);
}});

</script>

</body>
</html>";

        MapView.Source = new HtmlWebViewSource
        {
            Html = html
        };
    }

    private void MapView_Navigating(
        object sender,
        WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("nontonio://location"))
            return;

        e.Cancel = true;

        try
        {
            var uri = new Uri(e.Url);

            var query =
                System.Web.HttpUtility
                .ParseQueryString(uri.Query);

            double.TryParse(
                query["lat"],
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out _lat);

            double.TryParse(
                query["lon"],
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out _lon);
        }
        catch
        {
        }
    }

    private async void OnPickImageClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var file =
                await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle =
                        "Selecciona imagen"
                    });

            if (file == null)
                return;

            _image = file.FullPath;

            StoreImage.Source =
                ImageSource.FromFile(file.FullPath);
        }
        catch
        {
        }
    }

    private async void OnSaveClicked(
        object sender,
        EventArgs e)
    {
        var user =
            AuthService.GetCurrentUser();

        if (string.IsNullOrWhiteSpace(user))
            return;

        var store = new SellerStore
        {
            OwnerUser = user,

            StoreName =
                StoreNameEntry.Text ?? "",

            Phone =
                PhoneEntry.Text ?? "",

            Description =
                DescriptionEntry.Text ?? "",

            PhotoUrl = _image,

            Lat = _lat,
            Lon = _lon,

            Address =
                $"{_lat:F5}, {_lon:F5}",

            Schedule =
                $"{OpenPicker.SelectedItem} - {ClosePicker.SelectedItem}",

            Earnings = 0,

            TotalOrders = 0
        };

        await SellerStoreService.SaveAsync(store);

        await DisplayAlert(
            "Éxito",
            "Tienda registrada correctamente",
            "OK");

        Application.Current!.Windows[0].Page =
            new NavigationPage(
                new VendedorDashboardPage());
    }
}