using System;
using System.Collections.Generic;
using System.Linq;

namespace appP.A.Services
{
    public class Sucursal
    {
        public string Nombre { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public static class SucursalesService
    {
        private static readonly List<Sucursal> ListaSucursales = new List<Sucursal>
        {
            new Sucursal { Nombre = "Sucursal CDMX", Lat = 19.4326, Lon = -99.1332 },
            new Sucursal { Nombre = "Sucursal Monterrey", Lat = 25.6866, Lon = -100.3161 },
            new Sucursal { Nombre = "Sucursal Guadalajara", Lat = 20.6597, Lon = -103.3496 },
            new Sucursal { Nombre = "Sucursal Puebla", Lat = 19.0414, Lon = -98.2063 },
            new Sucursal { Nombre = "Sucursal Mérida", Lat = 20.9674, Lon = -89.5926 },
            new Sucursal { Nombre = "Sucursal Tijuana", Lat = 32.5149, Lon = -117.0382 },
            new Sucursal { Nombre = "Sucursal León", Lat = 21.1250, Lon = -101.6860 },
            new Sucursal { Nombre = "Sucursal Querétaro", Lat = 20.5888, Lon = -100.3899 },
            new Sucursal { Nombre = "Sucursal Cancún", Lat = 21.1619, Lon = -86.8515 },
            new Sucursal { Nombre = "Sucursal Toluca", Lat = 19.2827, Lon = -99.6557 }
        };

        public static Sucursal ObtenerMasCercana(double clienteLat, double clienteLon)
        {
            if (clienteLat == 0 && clienteLon == 0) return ListaSucursales[0];
            return ListaSucursales.OrderBy(s => Math.Sqrt(Math.Pow(s.Lat - clienteLat, 2) + Math.Pow(s.Lon - clienteLon, 2))).First();
        }
    }
}