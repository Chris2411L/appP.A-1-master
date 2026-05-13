using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace appP.A.Models
{
    public class Carrito : INotifyPropertyChanged
    {
        // =====================================
        // EVENTO
        // =====================================

        public event PropertyChangedEventHandler?
            PropertyChanged;

        // =====================================
        // LISTA PRODUCTOS
        // =====================================

        public ObservableCollection<Producto>
            Productos
        { get; set; } =
                new ObservableCollection<Producto>();

        // =====================================
        // TOTAL
        // =====================================

        public double Total =>
            Productos.Sum(p =>
                p.Precio * p.Cantidad);

        // =====================================
        // CONSTRUCTOR
        // =====================================

        public Carrito()
        {
            Productos.CollectionChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(Total));

                    if (e.NewItems != null)
                    {
                        foreach (Producto p in e.NewItems)
                        {
                            p.PropertyChanged +=
                                Producto_PropertyChanged;
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (Producto p in e.OldItems)
                        {
                            p.PropertyChanged -=
                                Producto_PropertyChanged;
                        }
                    }
                };
        }

        // =====================================
        // ACTUALIZAR TOTAL
        // =====================================

        private void Producto_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==
                    nameof(Producto.Cantidad)
                ||
                e.PropertyName ==
                    nameof(Producto.Precio))
            {
                OnPropertyChanged(nameof(Total));
            }
        }

        // =====================================
        // AGREGAR PRODUCTO
        // =====================================

        public void AgregarProducto(
            Producto p,
            int cantidad = 1)
        {
            if (p == null)
                return;

            var existente =
                Productos.FirstOrDefault(x =>
                    x.Id == p.Id);

            // YA EXISTE
            if (existente != null)
            {
                existente.Cantidad += cantidad;

                OnPropertyChanged(nameof(Total));

                return;
            }

            // NUEVO PRODUCTO
            var nuevo = new Producto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioAnterior = p.PrecioAnterior,
                Rating = p.Rating,
                Imagen = p.Imagen,
                Categoria = p.Categoria,
                Cantidad = cantidad,
                Stock = p.Stock
            };

            Productos.Add(nuevo);

            OnPropertyChanged(nameof(Total));
        }

        // =====================================
        // REDUCIR
        // =====================================

        public void ReducirCantidad(
            Producto p)
        {
            if (p == null)
                return;

            var existente =
                Productos.FirstOrDefault(x =>
                    x.Id == p.Id);

            if (existente == null)
                return;

            existente.Cantidad--;

            if (existente.Cantidad <= 0)
            {
                Productos.Remove(existente);
            }

            OnPropertyChanged(nameof(Total));
        }

        // =====================================
        // ELIMINAR
        // =====================================

        public void EliminarProductoDirecto(
            Producto p)
        {
            if (p == null)
                return;

            var existente =
                Productos.FirstOrDefault(x =>
                    x.Id == p.Id);

            if (existente != null)
            {
                Productos.Remove(existente);

                OnPropertyChanged(nameof(Total));
            }
        }

        // =====================================
        // QUITAR POR ID
        // =====================================

        public void QuitarProducto(
            int productoId)
        {
            var existente =
                Productos.FirstOrDefault(x =>
                    x.Id == productoId);

            if (existente != null)
            {
                Productos.Remove(existente);

                OnPropertyChanged(nameof(Total));
            }
        }

        // =====================================
        // VACIAR
        // =====================================

        public void Vaciar()
        {
            Productos.Clear();

            OnPropertyChanged(nameof(Total));
        }

        // =====================================
        // PROPERTY CHANGED
        // =====================================

        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}