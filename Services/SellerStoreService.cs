using SQLite;
using appP.A.Models;

namespace appP.A.Services
{
    public static class SellerStoreService
    {
        private static SQLiteAsyncConnection? _db;

        static async Task InitAsync()
        {
            if (_db != null)
                return;

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "sellerstores.db3");

            _db = new SQLiteAsyncConnection(path);

            await _db.CreateTableAsync<SellerStore>();
        }

        public static async Task SaveAsync(SellerStore store)
        {
            await InitAsync();

            if (store.Id == 0)
                await _db!.InsertAsync(store);
            else
                await _db!.UpdateAsync(store);
        }

        public static async Task<SellerStore?> GetByOwnerAsync(string user)
        {
            await InitAsync();

            return await _db!
                .Table<SellerStore>()
                .Where(x => x.OwnerUser == user)
                .FirstOrDefaultAsync();
        }
    }
}