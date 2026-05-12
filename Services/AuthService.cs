using appP.A.Models;
using SQLite;
using System.Security.Cryptography;
using System.Text;

namespace appP.A.Services
{
    public static class AuthService
    {
        private static SQLiteAsyncConnection? _db;

        private const string CurrentUserKey = "app_current_user";
        private const string CurrentRoleKey = "app_current_role";

        private static async Task InitAsync()
        {
            if (_db != null) return;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasePath = Path.Combine(appData, "NontonioUsers.db3");

            _db = new SQLiteAsyncConnection(databasePath);
            await _db.CreateTableAsync<User>();

            try
            {
                await _db.ExecuteAsync("ALTER TABLE User ADD COLUMN Rol TEXT NOT NULL DEFAULT 'Cliente'");
            }
            catch
            {
            }
        }

        public static async Task<(bool success, string error)> RegisterAsync(string username, string password)
        {
            return await RegisterAsync(username, password, "Cliente");
        }

        public static async Task<(bool success, string error)> RegisterAsync(string username, string password, string rol)
        {
            try
            {
                await InitAsync();

                if (string.IsNullOrWhiteSpace(username))
                    return (false, "Usuario vacío.");

                if (string.IsNullOrWhiteSpace(password))
                    return (false, "Contraseña vacía.");

                username = username.Trim();

                rol = rol switch
                {
                    "Repartidor" => "Repartidor",
                    "Vendedor" => "Vendedor",
                    _ => "Cliente"
                };

                var users = await _db!.Table<User>().ToListAsync();

                if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    return (false, "El usuario ya existe.");

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = Hash(password),
                    Rol = rol
                };

                await _db.InsertAsync(newUser);

                Preferences.Set(CurrentUserKey, username);
                Preferences.Set(CurrentRoleKey, rol);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                await InitAsync();

                var users = await _db!.Table<User>().ToListAsync();

                var user = users.FirstOrDefault(u =>
                    u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

                if (user != null && user.PasswordHash == Hash(password))
                {
                    Preferences.Set(CurrentUserKey, user.Username);
                    Preferences.Set(CurrentRoleKey, string.IsNullOrWhiteSpace(user.Rol) ? "Cliente" : user.Rol);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<User>> GetAllUsersAsync()
        {
            await InitAsync();
            return await _db!.Table<User>().ToListAsync();
        }

        public static async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                await InitAsync();

                var user = await _db!.FindAsync<User>(id);
                if (user == null) return false;

                await _db.DeleteAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string? GetCurrentUser()
        {
            return Preferences.Get(CurrentUserKey, null);
        }

        public static string GetCurrentRole()
        {
            return Preferences.Get(CurrentRoleKey, "Cliente");
        }

        public static async Task<string> GetCurrentUserRoleAsync()
        {
            await InitAsync();

            var currentUser = GetCurrentUser();

            if (string.IsNullOrWhiteSpace(currentUser))
                return "Cliente";

            var users = await _db!.Table<User>().ToListAsync();

            var user = users.FirstOrDefault(u =>
                u.Username.Equals(currentUser, StringComparison.OrdinalIgnoreCase));

            var rol = user?.Rol ?? Preferences.Get(CurrentRoleKey, "Cliente");

            if (string.IsNullOrWhiteSpace(rol))
                rol = "Cliente";

            Preferences.Set(CurrentRoleKey, rol);

            return rol;
        }

        public static async Task<bool> IsCurrentUserVendedorAsync()
        {
            var rol = await GetCurrentUserRoleAsync();
            return rol == "Vendedor";
        }

        public static void Logout()
        {
            Preferences.Remove(CurrentUserKey);
            Preferences.Remove(CurrentRoleKey);
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            var sb = new StringBuilder();

            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}