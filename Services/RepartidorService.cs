using appP.A.Models;
using SQLite;

namespace appP.A.Services
{
    public static class RepartidorService
    {
        private static SQLiteAsyncConnection? _db;

        private static async Task InitAsync()
        {
            if (_db != null) return;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(appData, "NontonioRepartidor.db3");

            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<RepartidorProfile>();
        }

        public static async Task<RepartidorProfile?> ObtenerAsync(string username)
        {
            await InitAsync();
            return await _db!.FindAsync<RepartidorProfile>(username);
        }

        public static async Task GuardarAsync(RepartidorProfile profile)
        {
            await InitAsync();
            await _db!.InsertOrReplaceAsync(profile);
        }
    }
}