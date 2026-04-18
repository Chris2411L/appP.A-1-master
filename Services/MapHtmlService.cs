using System.Globalization;

namespace appP.A.Services
{
    public static class MapHtmlService
    {
        public static string BuildLeafletHtml(double destLat, double destLon, double courierLat, double courierLon, double branchLat, double branchLon)
        {
            var destLatS = destLat.ToString(CultureInfo.InvariantCulture);
            var destLonS = destLon.ToString(CultureInfo.InvariantCulture);
            var courLatS = courierLat.ToString(CultureInfo.InvariantCulture);
            var courLonS = courierLon.ToString(CultureInfo.InvariantCulture);
            var branchLatS = branchLat.ToString(CultureInfo.InvariantCulture);
            var branchLonS = branchLon.ToString(CultureInfo.InvariantCulture);

            // Icons: branch (store), destination (home), courier (truck)
            var branchIcon = "https://img.icons8.com/fluency/48/000000/shop.png";
            var homeIcon = "https://img.icons8.com/fluency/48/000000/home.png";
            var courierIcon = "https://img.icons8.com/fluency/48/000000/delivery.png";

            // MapTiler raster tiles (256) with API key read from Preferences (falls back to the provided key)
            var mapTilerKey = Preferences.Get("maptiler_key", "b2vhT9Y6ylMbn6GP1WyV");
            var mapTilerTiles = $"https://api.maptiler.com/maps/openstreetmap/256/{{z}}/{{x}}/{{y}}.jpg?key={mapTilerKey}";

            var html = "<!DOCTYPE html>\n" +
                "<html>\n" +
                "<head>\n" +
                "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n" +
                "  <link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />\n" +
                "  <style>html,body,#map{height:100%;margin:0;padding:0}#map{height:100vh} .tile-error{position:absolute;left:8px;bottom:8px;padding:10px;background:#fff;color:#000;border-radius:6px;box-shadow:0 2px 6px rgba(0,0,0,.3);z-index:9999;max-width:60%;}</style>\n" +
                "</head>\n" +
                "<body>\n" +
                "  <div id=\"map\"></div>\n" +
                "  <script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>\n" +
                "  <script>\n" +
                "    var destLat = " + destLatS + ";\n" +
                "    var destLon = " + destLonS + ";\n" +
                "    var courLat = " + courLatS + ";\n" +
                "    var courLon = " + courLonS + ";\n" +
                "    var branchLat = " + branchLatS + ";\n" +
                "    var branchLon = " + branchLonS + ";\n" +
                "    var centerLat = (branchLat + destLat) / 2.0;\n" +
                "    var centerLon = (branchLon + destLon) / 2.0;\n" +
                "    var map = L.map('map').setView([centerLat, centerLon], 12);\n" +
                "    var tileUrl = '" + mapTilerTiles + "';\n" +
                "    L.tileLayer(tileUrl, { maxZoom: 20, attribution: '© MapTiler © OpenStreetMap contributors', tileSize:256 }).addTo(map);\n" +
                "    var branchIcon = L.icon({ iconUrl: '" + branchIcon + "', iconSize: [40,40], iconAnchor: [20,40] });\n" +
                "    var homeIcon = L.icon({ iconUrl: '" + homeIcon + "', iconSize: [40,40], iconAnchor: [20,40] });\n" +
                "    var carIcon = L.icon({ iconUrl: '" + courierIcon + "', iconSize: [40,40], iconAnchor: [20,40] });\n" +
                "    var branchMarker = L.marker([branchLat, branchLon], {icon: branchIcon}).addTo(map).bindPopup('Sucursal');\n" +
                "    var destMarker = L.marker([destLat, destLon], {icon: homeIcon}).addTo(map).bindPopup('Destino');\n" +
                "    var courierMarker = L.marker([courLat, courLon], {icon: carIcon}).addTo(map).bindPopup('Repartidor');\n" +
                "    function updateCourier(lat, lon) { try { courierMarker.setLatLng([lat, lon]); var bounds = L.latLngBounds([ [branchLat, branchLon], [destLat, destLon], [lat, lon] ]); map.fitBounds(bounds.pad(0.2)); } catch(e) {} }\n" +
                "    window.updateCourier = updateCourier;\n" +
                "  </script>\n" +
                "</body>\n" +
                "</html>\n";

            return html;
        }
    }
}
