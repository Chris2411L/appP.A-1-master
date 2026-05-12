using SQLite;

namespace appP.A.Models
{
    public class SellerStore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string OwnerUser { get; set; } = "";

        public string StoreName { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";

        public string Description { get; set; } = "";

        public string Schedule { get; set; } = "";

        public string PhotoUrl { get; set; } = "";

        public double Lat { get; set; }

        public double Lon { get; set; }

        public double Earnings { get; set; }

        public int TotalOrders { get; set; }

        public bool IsOpen { get; set; } = true;
    }
}