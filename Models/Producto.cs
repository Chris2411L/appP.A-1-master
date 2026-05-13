using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace appP.A.Models
{
    public class Producto : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public string Nombre { get; set; } =
            string.Empty;

        public string Descripcion { get; set; } =
            string.Empty;

        // =====================================
        // PRECIO
        // =====================================

        private double _precio;

        public double Precio
        {
            get => _precio;
            set
            {
                _precio = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public double PrecioAnterior { get; set; }

        public double Rating { get; set; }

        // =====================================
        // IMAGEN
        // =====================================

        private string _imagen =
            string.Empty;

        public string Imagen
        {
            get => _imagen;
            set
            {
                _imagen = value;
                OnPropertyChanged();
            }
        }

        public string Categoria { get; set; } =
            string.Empty;

        // =====================================
        // CANTIDAD CARRITO
        // =====================================

        private int _cantidad = 1;

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // =====================================
        // STOCK
        // =====================================

        private int _stock = 50;

        public int Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged();
            }
        }

        // =====================================
        // SUBTOTAL
        // =====================================

        public double Subtotal =>
            Precio * Cantidad;

        // =====================================
        // CONSTRUCTOR VACÍO
        // =====================================

        public Producto()
        {
        }

        // =====================================
        // CONSTRUCTOR
        // =====================================

        public Producto(
            string nombre,
            string descripcion,
            double precio,
            double precioAnterior,
            double rating,
            string imagen,
            string categoria)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            PrecioAnterior = precioAnterior;
            Rating = rating;
            Imagen = imagen;
            Categoria = categoria;

            Cantidad = 1;
            Stock = 50;
        }

        // =====================================
        // PROPERTY CHANGED
        // =====================================

        public event PropertyChangedEventHandler?
            PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}