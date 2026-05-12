using System.Globalization;
using Microsoft.Maui.Storage;

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
    <style>
        html, body, #map {{
            height:100%;
            margin:0;
            padding:0;
            background:#eee;
        }}
    </style>
</head>
<body>
    <div id='map'></div>

    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

    <script>
        var map, truck;

        async function init() {{
            map = L.map('map').setView([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], 15);

            L.tileLayer('https://api.maptiler.com/maps/openstreetmap/256/{{z}}/{{x}}/{{y}}.jpg?key={mapTilerKey}').addTo(map);

            var iconStore = L.icon({{
                iconUrl: 'https://img.icons8.com/fluency/48/shop.png',
                iconSize: [40,40],
                iconAnchor: [20,40]
            }});

            var iconHome = L.icon({{
                iconUrl: 'https://img.icons8.com/fluency/48/home.png',
                iconSize: [40,40],
                iconAnchor: [20,40]
            }});

            var iconTruck = L.icon({{
                iconUrl: 'https://img.icons8.com/fluency/48/delivery.png',
                iconSize: [45,45],
                iconAnchor: [22,45]
            }});

            L.marker([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], {{icon: iconStore}})
                .addTo(map)
                .bindPopup('Sucursal');

            L.marker([{destLat.ToString(culture)}, {destLon.ToString(culture)}], {{icon: iconHome}})
                .addTo(map)
                .bindPopup('Casa del Cliente');

            truck = L.marker([{branchLat.ToString(culture)}, {branchLon.ToString(culture)}], {{icon: iconTruck}})
                .addTo(map);

            try {{
                const url = `https://router.project-osrm.org/route/v1/driving/{branchLon.ToString(culture)},{branchLat.ToString(culture)};{destLon.ToString(culture)},{destLat.ToString(culture)}?overview=full&geometries=geojson`;

                const response = await fetch(url);
                const data = await response.json();

                const routeCoords = data.routes[0].geometry.coordinates.map(c => [c[1], c[0]]);

                L.polyline(routeCoords, {{
                    color: '#3498db',
                    weight: 5,
                    opacity: 0.5
                }}).addTo(map);

                map.fitBounds(L.latLngBounds(routeCoords).pad(0.2));
                startDelivery(routeCoords);
            }} catch(e) {{
                console.error('No se pudo trazar la ruta por carretera:', e);

                var fallback = [
                    [{branchLat.ToString(culture)}, {branchLon.ToString(culture)}],
                    [{destLat.ToString(culture)}, {destLon.ToString(culture)}]
                ];

                L.polyline(fallback, {{
                    color: '#3498db',
                    weight: 5,
                    opacity: 0.5,
                    dashArray: '8,8'
                }}).addTo(map);

                map.fitBounds(L.latLngBounds(fallback).pad(0.2));
                startDelivery(fallback);
            }}
        }}

        function startDelivery(path) {{
            const duration = 180000;
            const start = performance.now();

            function animate(now) {{
                const elapsed = now - start;
                const progress = Math.min(elapsed / duration, 1);
                const index = Math.floor(progress * (path.length - 1));

                if (path[index]) {{
                    truck.setLatLng(path[index]);
                }}

                if (progress < 1) {{
                    requestAnimationFrame(animate);
                }} else {{
                    truck.bindPopup('¡Pedido Entregado!').openPopup();
                }}
            }}

            requestAnimationFrame(animate);
        }}

        window.onload = init;
    </script>
</body>
</html>";
        }

        public static string BuildRepartidorSimulationHtml(
            double riderLat,
            double riderLon,
            double branchLat,
            double branchLon,
            double destLat,
            double destLon)
        {
            var culture = CultureInfo.InvariantCulture;
            var mapTilerKey = Preferences.Get("maptiler_key", "b2vhT9Y6ylMbn6GP1WyV");

            return $@"
<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
<style>
html,body,#map {{
    height:100%;
    margin:0;
    padding:0;
    background:#eee;
}}
.info {{
    position:absolute;
    top:12px;
    left:12px;
    right:12px;
    z-index:9999;
    background:white;
    padding:10px;
    border-radius:14px;
    font-family:Arial;
    font-weight:bold;
    box-shadow:0 4px 16px #0003;
}}
</style>
</head>
<body>
<div class='info' id='info'>🏍️ Buscando ruta hacia la tienda...</div>
<div id='map'></div>

<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

<script>
let map, riderMarker;

async function init() {{
    const riderStart = [{riderLat.ToString(culture)}, {riderLon.ToString(culture)}];
    const store = [{branchLat.ToString(culture)}, {branchLon.ToString(culture)}];
    const home = [{destLat.ToString(culture)}, {destLon.ToString(culture)}];

    map = L.map('map').setView(store, 15);

    L.tileLayer('https://api.maptiler.com/maps/openstreetmap/256/{{z}}/{{x}}/{{y}}.jpg?key={mapTilerKey}').addTo(map);

    const iconStore = L.icon({{
        iconUrl:'https://img.icons8.com/fluency/48/shop.png',
        iconSize:[42,42],
        iconAnchor:[21,42]
    }});

    const iconHome = L.icon({{
        iconUrl:'https://img.icons8.com/fluency/48/home.png',
        iconSize:[42,42],
        iconAnchor:[21,42]
    }});

    const iconRider = L.icon({{
        iconUrl:'https://img.icons8.com/fluency/48/delivery-scooter.png',
        iconSize:[48,48],
        iconAnchor:[24,48]
    }});

    L.marker(store, {{icon: iconStore}}).addTo(map).bindPopup('🏪 Tienda / Sucursal');
    L.marker(home, {{icon: iconHome}}).addTo(map).bindPopup('🏠 Casa del cliente');

    riderMarker = L.marker(riderStart, {{icon: iconRider}}).addTo(map).bindPopup('🏍️ Tú / Repartidor');

    const route1 = await getRoute(riderStart, store);
    const route2 = await getRoute(store, home);

    const all = route1.concat(route2);

    if (all.length > 0) {{
        map.fitBounds(L.latLngBounds(all).pad(0.22));
    }}

    if (route1.length > 0) {{
        L.polyline(route1, {{
            color:'#111',
            weight:5,
            opacity:0.55
        }}).addTo(map);
    }}

    if (route2.length > 0) {{
        L.polyline(route2, {{
            color:'#3498db',
            weight:5,
            opacity:0.65
        }}).addTo(map);
    }}

    document.getElementById('info').innerText = '🏍️ Camino a la tienda...';
    await animateRoute(route1, 9000);

    document.getElementById('info').innerText = '📦 Pedido recogido. Camino al cliente...';
    await animateRoute(route2, 13000);

    document.getElementById('info').innerText = '✅ Pedido entregado';
    riderMarker.bindPopup('✅ ¡Pedido entregado!').openPopup();

    window.location.href = 'nontonio://delivered';
}}

async function getRoute(from, to) {{
    try {{
        const url = `https://router.project-osrm.org/route/v1/driving/${{from[1]}},${{from[0]}};${{to[1]}},${{to[0]}}?overview=full&geometries=geojson`;

        const response = await fetch(url);
        const data = await response.json();

        return data.routes[0].geometry.coordinates.map(c => [c[1], c[0]]);
    }} catch(e) {{
        return [from, to];
    }}
}}

function animateRoute(path, duration) {{
    return new Promise(resolve => {{
        if (!path || path.length === 0) {{
            resolve();
            return;
        }}

        const start = performance.now();

        function animate(now) {{
            const progress = Math.min((now - start) / duration, 1);
            const index = Math.floor(progress * (path.length - 1));

            if (path[index]) {{
                riderMarker.setLatLng(path[index]);
            }}

            if (progress < 1) {{
                requestAnimationFrame(animate);
            }} else {{
                resolve();
            }}
        }}

        requestAnimationFrame(animate);
    }});
}}

window.onload = init;
</script>
</body>
</html>";
        }
    }
}