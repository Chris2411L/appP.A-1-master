using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace appP.A.Models
{
    public class Carrito : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Producto> Productos { get; } = new ObservableCollection<Producto>();

        public double Total => Productos.Sum(p => p.Precio * p.Cantidad);

        public Carrito()
        {
            Productos.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Total));
                if (e.NewItems != null)
                {
                    foreach (Producto p in e.NewItems) p.PropertyChanged += Producto_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (Producto p in e.OldItems) p.PropertyChanged -= Producto_PropertyChanged;
                }
            };
        }

        private void Producto_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Producto.Cantidad) || e.PropertyName == nameof(Producto.Precio))
            {
                OnPropertyChanged(nameof(Total));
            }
        }

        public void AgregarProducto(Producto p, int cantidad = 1)
        {
            if (p == null) return;

            var existente = Productos.FirstOrDefault(x =>
                (x.Id != 0 && p.Id != 0 && x.Id == p.Id) ||
                (x.Id == 0 && x.Nombre == p.Nombre));

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                OnPropertyChanged(nameof(Total));
            }
            else
            {
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
                    Cantidad = cantidad
                };

                Productos.Add(nuevo);
            }
        }

        // Nueva función: Reduce la cantidad y lo elimina si llega a 0
        public void ReducirCantidad(Producto p)
        {
            if (p == null) return;

            var existente = Productos.FirstOrDefault(x => x.Id == p.Id || x.Nombre == p.Nombre);
            if (existente != null)
            {
                existente.Cantidad--;
                if (existente.Cantidad <= 0)
                {
                    Productos.Remove(existente);
                }
                OnPropertyChanged(nameof(Total));
            }
        }

        // Nueva función: Elimina el producto por completo sin importar la cantidad
        public void EliminarProductoDirecto(Producto p)
        {
            if (p == null) return;
            var existente = Productos.FirstOrDefault(x => x.Id == p.Id || x.Nombre == p.Nombre);
            if (existente != null)
            {
                Productos.Remove(existente);
                OnPropertyChanged(nameof(Total));
            }
        }

        public void QuitarProducto(int productoId)
        {
            var existente = Productos.FirstOrDefault(x => x.Id == productoId);
            if (existente != null)
            {
                Productos.Remove(existente);
                OnPropertyChanged(nameof(Total));
            }
        }

        public void Vaciar()
        {
            Productos.Clear();
            OnPropertyChanged(nameof(Total));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}