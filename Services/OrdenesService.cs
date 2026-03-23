using appP.A.Models;
using SQLite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace appP.A.Services
{
    public static class OrdenesService
    {
        private static SQLiteAsyncConnection _db;

        private static async Task InitAsync()
        {
            if (_db != null) return;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(appData, "NontonioOrdenes.db3");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<Orden>();
        }

        public static async Task GuardarOrdenAsync(Orden orden)
        {
            await InitAsync();
            await _db.InsertAsync(orden);
        }

        public static async Task<List<Orden>> ObtenerOrdenesUsuarioAsync(string usuario)
        {
            await InitAsync();
            // Devuelve las compras del usuario ordenadas de la más reciente a la más antigua
            return await _db.Table<Orden>()
                            .Where(o => o.Usuario == usuario)
                            .OrderByDescending(o => o.Fecha)
                            .ToListAsync();
        }
    }
}