using System.Globalization;

namespace appP.A.Services
{
    public static class MapHtmlService
    {
        public static string BuildLeafletHtml(double destLat, double destLon, double branchLat, double branchLon)
        {
            var culture = CultureInfo.InvariantCulture;
            var mapTilerKey = Preferences.Get("maptiler_key", "b2vhT9Y6ylMbn6GP1WyV");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <style>html,body,#map {{ height:100%; margin:0; padding:0; background:#eee; }}</style>
</head>
<body>
    <div id='map'></div>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script>
        var map, truck;
        async function init() {{
            map = L.map('map').setView([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], 15);
            L.tileLayer('https://api.maptiler.com/maps/openstreetmap/256/{{z}}/{{x}}/{{y}}.jpg?key={mapTilerKey}').addTo(map);

            // Iconos con anclaje en la base (Punto 20,40 para que la punta esté en la coordenada)
            var iconStore = L.icon({{ iconUrl: 'https://img.icons8.com/fluency/48/shop.png', iconSize: [40,40], iconAnchor: [20,40] }});
            var iconHome = L.icon({{ iconUrl: 'https://img.icons8.com/fluency/48/home.png', iconSize: [40,40], iconAnchor: [20,40] }});
            var iconTruck = L.icon({{ iconUrl: 'https://img.icons8.com/fluency/48/delivery.png', iconSize: [45,45], iconAnchor: [22,45] }});

            L.marker([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], {{icon: iconStore}}).addTo(map).bindPopup('Sucursal');
            L.marker([{destLat.ToString(culture)}, {destLon.ToString(culture)}], {{icon: iconHome}}).addTo(map).bindPopup('Casa del Cliente');
            truck = L.marker([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], {{icon: iconTruck}}).addTo(map);

            // Obtener ruta por carreteras reales (OSRM)
            try {{
                const url = `https://router.project-osrm.org/route/v1/driving/{branchLon.ToString(culture)},{branchLat.ToString(culture)};{destLon.ToString(culture)},{destLat.ToString(culture)}?overview=full&geometries=geojson`;
                const response = await fetch(url);
                const data = await response.json();
                const routeCoords = data.routes[0].geometry.coordinates.map(c => [c[1], c[0]]);
                
                L.polyline(routeCoords, {{color: '#3498db', weight: 5, opacity: 0.5}}).addTo(map);
                map.fitBounds(L.latLngBounds(routeCoords).pad(0.2));

                startDelivery(routeCoords);
            }} catch(e) {{
                console.error('No se pudo trazar la ruta por carretera:', e);
            }}
        }}

        function startDelivery(path) {{
            const duration = 180000; // 3 Minutos exactos
            const start = performance.now();

            function animate(now) {{
                const elapsed = now - start;
                const progress = Math.min(elapsed / duration, 1);
                const index = Math.floor(progress * (path.length - 1));
                
                if (path[index]) {{
                    truck.setLatLng(path[index]);
                }}

                if (progress < 1) requestAnimationFrame(animate);
                else truck.bindPopup('¡Pedido Entregado!').openPopup();
            }}
            requestAnimationFrame(animate);
        }}

        window.onload = init;
    </script>
</body>
</html>";
        }
    }
}