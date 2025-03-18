using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        // Método para gerar o hash da senha com salt
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Gerar um salt aleatório
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }

                // Combinar o salt com a senha
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordWithSalt = new byte[salt.Length + passwordBytes.Length];
                Array.Copy(salt, 0, passwordWithSalt, 0, salt.Length);
                Array.Copy(passwordBytes, 0, passwordWithSalt, salt.Length, passwordBytes.Length);

                // Gerar o hash da senha com o salt
                byte[] hashBytes = sha256.ComputeHash(passwordWithSalt);

                // Converter o salt e o hash para uma string Base64
                string saltString = Convert.ToBase64String(salt);
                string hashString = Convert.ToBase64String(hashBytes);

                // Armazenar o salt e o hash juntos para a verificação posterior
                return $"{saltString}:{hashString}";
            }
        }

        // Método para verificar a senha fornecida com salt
        public bool VerifyPassword(string password, string storedHash)
        {
            // Separar o salt e o hash armazenados
            var parts = storedHash.Split(':');
            string storedSalt = parts[0];
            string storedHashValue = parts[1];

            // Converter o salt de volta para bytes
            byte[] salt = Convert.FromBase64String(storedSalt);

            // Combinar o salt com a senha fornecida
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] passwordWithSalt = new byte[salt.Length + passwordBytes.Length];
            Array.Copy(salt, 0, passwordWithSalt, 0, salt.Length);
            Array.Copy(passwordBytes, 0, passwordWithSalt, salt.Length, passwordBytes.Length);

            using (SHA256 sha256 = SHA256.Create())
            {
                // Gerar o hash da senha fornecida com o salt
                byte[] hashBytes = sha256.ComputeHash(passwordWithSalt);

                // Converter o hash gerado para Base64
                string hashedInputPassword = Convert.ToBase64String(hashBytes);

                // Comparar o hash gerado com o hash armazenado
                return hashedInputPassword == storedHashValue;
            }
        }
    }
}
