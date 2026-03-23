using SQLite;
using System;

namespace appP.A.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Username { get; set; } = string.Empty;

        // Aquí guardaremos la contraseña encriptada por seguridad
        public string PasswordHash { get; set; } = string.Empty;
    }
}