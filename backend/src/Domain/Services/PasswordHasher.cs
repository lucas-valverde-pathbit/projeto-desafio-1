using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }

        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            using (var sha256 = SHA256.Create())
            {
                var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hashBytes[i]) return false;
                }
            }

            return true;
        }
    }
}
