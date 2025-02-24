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
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;
        
        public UserService(AppDbContext context) : base(context) {}

        public async Task<User?> Authenticate(string username, string password)
        {
            var user = await _dbSet
                .FirstOrDefaultAsync(u => u.UserEmail == username);

            if (user == null)
            {
                Console.WriteLine("Usuário não encontrado");
                return null;
            }

            // Check if account is locked
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                Console.WriteLine($"Conta bloqueada até {user.LockoutEnd}");
                return null;
            }

            Console.WriteLine($"Usuário encontrado: {user.UserEmail}");


            string hashedPassword = ComputeSha256Hash(password);
            
            Console.WriteLine($"Senha recebida: {password}");
            Console.WriteLine($"Hash gerado: {hashedPassword}");
            Console.WriteLine($"Hash armazenado: {user.UserPassword}");
            
            if (user.UserPassword != hashedPassword)
            {
                Console.WriteLine("Hashes não coincidem");
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    Console.WriteLine($"Conta bloqueada por {LockoutMinutes} minutos");
                }
                await _context.SaveChangesAsync();
                return null;
            }

            Console.WriteLine("Autenticação bem-sucedida");



            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            return user;
        }

        private string ComputeSha256Hash(string rawData)
        {
            // Remove espaços em branco no início e fim
            string trimmedData = rawData.Trim();
            
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(trimmedData));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task Create(User user)
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
