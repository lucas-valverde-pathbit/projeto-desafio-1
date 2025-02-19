using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;



namespace Infrastructure.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        public UserService(AppDbContext context) : base(context) {}



        public async Task<User?> Authenticate(string username, string password)
        {
            string hashedPassword = ComputeSha256Hash(password);
            
            return await _dbSet.FirstOrDefaultAsync(u => u.UserEmail == username && u.UserPassword == hashedPassword);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create()) // Alterado para usar SHA256.Create()
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString(); // Corrigido para usar o ponto, não a vírgula
            }
        }
    }
}
