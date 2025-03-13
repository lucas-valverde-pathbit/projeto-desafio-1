using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        // Método para hash da senha sem Salt
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);

                // Converter o hash para uma string em base64
                return Convert.ToBase64String(hashBytes); 
            }
        }

        // Método para verificar a senha fornecida sem Salt
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Gerar o hash da senha fornecida
            string hashedInputPassword = HashPassword(password);

            // Comparar o hash gerado com o hash armazenado
            return hashedInputPassword == hashedPassword;
        }
    }
}
