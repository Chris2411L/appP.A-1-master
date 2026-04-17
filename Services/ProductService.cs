using appP.A.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace appP.A.Services
{
    public class ProductRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public double Precio { get; set; }
        public double PrecioAnterior { get; set; }
        public double Rating { get; set; }
        public string Imagen { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
    }

    public static class ProductService
    {
        private static SQLiteAsyncConnection _db;

        private static async Task InitAsync()
        {
            if (_db != null) return;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(appData, "NontonioProducts.db3");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<ProductRecord>();
        }

        public static async Task<List<ProductRecord>> GetAllAsync()
        {
            await InitAsync();
            return await _db.Table<ProductRecord>().Where(p => !p.Deleted).OrderBy(p => p.Nombre).ToListAsync();
        }

        public static async Task<int> AddAsync(Producto p, string adminUser)
        {
            await InitAsync();
            var rec = new ProductRecord
            {
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioAnterior = p.PrecioAnterior,
                Rating = p.Rating,
                Imagen = p.Imagen,
                Categoria = p.Categoria,
                Stock = p.Stock,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUser ?? "admin"
            };
            await _db.InsertAsync(rec);
            return rec.Id;
        }

        public static async Task<bool> DeleteAsync(int productId, string adminUser)
        {
            await InitAsync();
            var rec = await _db.FindAsync<ProductRecord>(productId);
            if (rec == null) return false;
            rec.Deleted = true;
            rec.DeletedAt = DateTime.UtcNow;
            rec.DeletedBy = adminUser ?? "admin";
            await _db.UpdateAsync(rec);
            return true;
        }

        public static async Task<bool> UpdateAsync(Producto p)
        {
            await InitAsync();
            var rec = await _db.FindAsync<ProductRecord>(p.Id);
            if (rec == null) return false;
            rec.Nombre = p.Nombre;
            rec.Descripcion = p.Descripcion;
            rec.Precio = p.Precio;
            rec.PrecioAnterior = p.PrecioAnterior;
            rec.Rating = p.Rating;
            rec.Imagen = p.Imagen;
            rec.Categoria = p.Categoria;
            rec.Stock = p.Stock;
            await _db.UpdateAsync(rec);
            return true;
        }

        public static async Task SyncToAppDataAsync()
        {
            await InitAsync();
            var items = await _db.Table<ProductRecord>().Where(p => !p.Deleted).ToListAsync();

            foreach (var r in items)
            {
                var existing = AppData.Categorias.SelectMany(c => c.Productos).FirstOrDefault(p => p.Id == r.Id || (!string.IsNullOrEmpty(p.Nombre) && p.Nombre == r.Nombre));
                if (existing != null)
                {
                    existing.Nombre = r.Nombre;
                    existing.Descripcion = r.Descripcion;
                    existing.Precio = r.Precio;
                    existing.PrecioAnterior = r.PrecioAnterior;
                    existing.Rating = r.Rating;
                    existing.Imagen = r.Imagen;
                    existing.Categoria = r.Categoria;
                    existing.Stock = r.Stock;
                    existing.Id = r.Id;
                }
                else
                {
                    var cat = AppData.Categorias.FirstOrDefault(c => c.Nombre == r.Categoria);
                    if (cat == null)
                    {
                        cat = new Categoria(r.Categoria, "");
                        AppData.Categorias.Add(cat);
                    }

                    var prod = new Producto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Precio = r.Precio,
                        PrecioAnterior = r.PrecioAnterior,
                        Rating = r.Rating,
                        Imagen = r.Imagen,
                        Categoria = r.Categoria,
                        Stock = r.Stock
                    };
                    cat.Productos.Add(prod);
                }
            }
        }
    }
}
