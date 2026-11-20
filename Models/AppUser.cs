using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace GrooveyTunes.Models
{
    // One row in the AppUsers table
    public class AppUser
    {
        public int AppUserId { get; set; }

        [Required, StringLength(20)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }          // always stored in lower case

        [Required]
        public string PasswordHash { get; set; }   // never the real password

        [Required]
        public string Salt { get; set; }

        [Required, StringLength(20)]
        public string Role { get; set; }           // "User" or "Admin"

        public DateTime CreatedAt { get; set; }
    }

    // Small helper that turns a password into a salted hash
    public static class PasswordHelper
    {
        public static string CreateSalt()
        {
            byte[] bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public static string Hash(string password, string salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
            }
        }
    }
}
