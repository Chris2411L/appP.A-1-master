using SQLite;

namespace appP.A.Models
{
    public class RepartidorProfile
    {
        [PrimaryKey]
        public string Username { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Domicilio { get; set; } = string.Empty;
        public string FotoPath { get; set; } = string.Empty;

        public double Saldo { get; set; }
        public int Viajes { get; set; }
    }
}