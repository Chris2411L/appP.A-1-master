using appP.A.Models;
using SQLite;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace appP.A.Services
{
    // Esta es la tabla que se creará en la base de datos
    public class ProductoStock
    {
        [PrimaryKey]
        public int ProductoId { get; set; }
        public int Stock { get; set; }
    }

    public static class InventarioService
    {
        private static SQLiteAsyncConnection _db;

        private static async Task InitAsync()
        {
            if (_db != null) return;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasePath = Path.Combine(appData, "NontonioInventario.db3");
            _db = new SQLiteAsyncConnection(databasePath);
            await _db.CreateTableAsync<ProductoStock>();
        }

        // Esta función lee la base de datos y le pone las cantidades correctas a los productos
        public static async Task SincronizarStockAsync()
        {
            await InitAsync();
            var stocksGuardados = await _db.Table<ProductoStock>().ToListAsync();

            foreach (var categoria in AppData.Categorias)
            {
                foreach (var producto in categoria.Productos)
                {
                    var guardado = stocksGuardados.FirstOrDefault(s => s.ProductoId == producto.Id);
                    if (guardado != null)
                    {
                        producto.Stock = guardado.Stock;
                    }
                    else
                    {
                        // Si es un producto nuevo, lo guardamos en la base de datos con su stock inicial
                        await _db.InsertAsync(new ProductoStock { ProductoId = producto.Id, Stock = producto.Stock });
                    }
                }
            }
        }

        // Esta función se llamará cuando el Admin modifique el stock
        public static async Task ActualizarStockAsync(int productoId, int nuevoStock)
        {
            await InitAsync();
            var item = await _db.Table<ProductoStock>().Where(x => x.ProductoId == productoId).FirstOrDefaultAsync();

            if (item != null)
            {
                item.Stock = nuevoStock;
                await _db.UpdateAsync(item);
            }
            else
            {
                await _db.InsertAsync(new ProductoStock { ProductoId = productoId, Stock = nuevoStock });
            }

            // Actualizar también en la memoria visible de la app
            foreach (var cat in AppData.Categorias)
            {
                var prod = cat.Productos.FirstOrDefault(p => p.Id == productoId);
                if (prod != null)
                {
                    prod.Stock = nuevoStock;
                    break;
                }
            }
        }
    }
}