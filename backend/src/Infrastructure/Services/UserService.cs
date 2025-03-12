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
          public async Task UpdateUserAsync(Guid userId, string userName, string userEmail, string password, UserRole role, string? customerName, string? customerEmail)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Atualiza as informações do usuário
            user.UserName = userName;
            user.UserEmail = userEmail;
            user.UserPassword = password;
            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            // Se for um cliente, também atualiza as informações no Customer
            if (role == UserRole.CLIENTE)
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer != null)
                {
                    // Atualiza informações do cliente
                    customer.CustomerName = customerName;
                    customer.CustomerEmail = customerEmail;
                }
                else
                {
                    // Se o cliente não existir, cria um novo
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerName = customerName,
                        CustomerEmail = customerEmail,
                        UserId = userId,
                        User = user
                    };
                    await _customerRepository.AddAsync(customer);
                }
            }

            await _userRepository.UpdateAsync(user);

            if (role == UserRole.CLIENTE)
            {
                await _customerRepository.UpdateAsync(customer);
            }
        }
    }
}
