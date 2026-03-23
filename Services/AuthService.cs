using appP.A.Models;
using SQLite;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Avoid direct dependency on Microsoft.Maui.Storage to allow building in environments
// where MAUI workloads are not installed. Use LocalApplicationData instead.

namespace appP.A.Services
{
    public static class AuthService
    {
        private static SQLiteAsyncConnection _db;
        private const string CurrentUserKey = "app_current_user";

        private static async Task InitAsync()
        {
            if (_db != null) return;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasePath = Path.Combine(appData, "NontonioUsers.db3");
            _db = new SQLiteAsyncConnection(databasePath);
            await _db.CreateTableAsync<User>();
        }

        public static async Task<(bool success, string error)> RegisterAsync(string username, string password)
        {
            // Envolver TODO en un try-catch evita que la app se cierre si algo falla
            try
            {
                await InitAsync();

                if (string.IsNullOrWhiteSpace(username)) return (false, "Usuario vacío.");
                if (string.IsNullOrWhiteSpace(password)) return (false, "Contraseña vacía.");
                username = username.Trim();

                if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
                    return (false, "El nombre 'admin' está reservado.");

                // CORRECCIÓN ANTI-CRASH: Evitamos usar .ToLower() directamente en la base de datos.
                // Obtenemos los usuarios y los comparamos de forma segura en memoria.
                var todosLosUsuarios = await _db.Table<User>().ToListAsync();
                var existingUser = todosLosUsuarios.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (existingUser != null)
                    return (false, "El usuario ya existe.");

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = Hash(password)
                };

                await _db.InsertAsync(newUser);
                Preferences.Set(CurrentUserKey, username);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                // Si ocurre un fallo, la app no se cierra, sino que te avisa en rojo
                return (false, $"Error interno: {ex.Message}");
            }
        }

        public static async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                await InitAsync();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;
                username = username.Trim();

                // CORRECCIÓN ANTI-CRASH AQUÍ TAMBIÉN
                var todosLosUsuarios = await _db.Table<User>().ToListAsync();
                var user = todosLosUsuarios.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    var providedHash = Hash(password);
                    if (user.PasswordHash == providedHash)
                    {
                        Preferences.Set(CurrentUserKey, user.Username);
                        return true;
                    }
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
            return await _db.Table<User>().ToListAsync();
        }

        public static async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                await InitAsync();
                var user = await _db.FindAsync<User>(id);
                if (user == null) return false;

                await _db.DeleteAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Logout()
        {
            Preferences.Remove(CurrentUserKey);
        }

        public static string? GetCurrentUser()
        {
            return Preferences.Get(CurrentUserKey, null);
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}