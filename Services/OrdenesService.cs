using appP.A.Models;
using SQLite;

namespace appP.A.Services
{
    public static class OrdenesService
    {
        private static SQLiteAsyncConnection? _db;

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

            if (orden.Id > 0)
                await _db!.InsertOrReplaceAsync(orden);
            else
                await _db!.InsertAsync(orden);
        }

        public static async Task ActualizarOrdenAsync(Orden orden)
        {
            await InitAsync();
            await _db!.UpdateAsync(orden);
        }

        public static async Task<Orden?> ObtenerOrdenPorIdAsync(int id)
        {
            await InitAsync();

            return await _db!.Table<Orden>()
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<Orden>> ObtenerOrdenesUsuarioAsync(string usuario)
        {
            await InitAsync();

            return await _db!.Table<Orden>()
                .Where(o => o.Usuario == usuario)
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
        }

        public static async Task<List<Orden>> ObtenerTodasOrdenesAsync()
        {
            await InitAsync();

            return await _db!.Table<Orden>()
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
        }
    }
}