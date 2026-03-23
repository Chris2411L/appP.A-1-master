using SQLite;
using System;

namespace appP.A.Models
{
    public class Orden
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public double Total { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;

        // Aquí guardaremos un texto con el resumen de lo que compró (ej. "2x Sabritas, 1x Coca-Cola")
        public string Detalles { get; set; } = string.Empty;
    }
}