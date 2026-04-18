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

        // Nuevas propiedades para seguimiento
        // Estado: Preparando, En camino, En reparto, Entregado
        public string Status { get; set; } = "Preparando";

        // Posición actual del repartidor
        public double CurrentLat { get; set; }
        public double CurrentLon { get; set; }

        // Posición destino (cliente)
        public double DestLat { get; set; }
        public double DestLon { get; set; }

        // Sucursal asignada para preparar/entregar (opcional)
        public double BranchLat { get; set; }
        public double BranchLon { get; set; }
        public string BranchName { get; set; } = string.Empty;

        // Historial de eventos (JSON simple)
        public string HistoryJson { get; set; } = string.Empty;

        // Estimación de entrega (opcional)
        public DateTime? EstimatedDelivery { get; set; }
    }
}