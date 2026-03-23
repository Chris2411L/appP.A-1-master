using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace appP.A.Models
{
    public class Producto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public double Precio { get; set; }
        public double PrecioAnterior { get; set; }
        public double Rating { get; set; }
        public string Imagen { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;

        // Propiedad original para el Carrito
        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
        }

        public double Subtotal => Precio * Cantidad;

        // NUEVO: Propiedad para el Almacén (Inventario)
        private int _stock;
        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public Producto() { }

        public Producto(string nombre, string descripcion, double precio, double precioAnterior, double rating, string imagen, string categoria)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            PrecioAnterior = precioAnterior;
            Rating = rating;
            Imagen = imagen;
            Categoria = categoria;
            Stock = 50; // Stock inicial por defecto si es nuevo
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}