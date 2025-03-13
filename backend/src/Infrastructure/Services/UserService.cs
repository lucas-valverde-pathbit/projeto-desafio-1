using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Domain.Models;
using Domain.Services;
using Domain.Repositories;
using Domain.DTOs;
using Infrastructure.Data;

namespace Infrastructure.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(AppDbContext context, IUserRepository userRepository, ICustomerRepository customerRepository, IPasswordHasher passwordHasher) : base(context)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
        }

public async Task<User> Authenticate(string username, string password)

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

public async Task UpdateUserAsync(Guid userId, EditProfileDto editProfileDto, UserRole role)


{
    // Recuperar o usuário
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
    {
        throw new Exception("Usuário não encontrado.");
    }

    // Atualizar as informações do usuário
    user.UserName = editProfileDto.Name;
    user.UserEmail = editProfileDto.Email;
    
    // Se a senha for fornecida, atualize a senha do usuário
    if (!string.IsNullOrEmpty(editProfileDto.NewPassword))
    {
        user.UserPassword = _passwordHasher.HashPassword(editProfileDto.NewPassword); // Lógica para hash da senha
    }

     user.Role = UserRole.CLIENTE;  // Atribuindo o novo papel do usuário

    user.UpdatedAt = DateTime.UtcNow;

    // Salvar alterações no usuário
    await _userRepository.UpdateAsync(user);

    // Atualizar os dados do cliente se houver
    var customerName = editProfileDto.Name;  // CustomerName igual a UserName
    var customerEmail = editProfileDto.Email;  // CustomerEmail igual a UserEmail

    var customer = await _customerRepository.GetByUserIdAsync(userId);
    if (customer != null)
    {
        customer.CustomerName = customerName;
        customer.CustomerEmail = customerEmail;
        await _customerRepository.UpdateAsync(customer);
    }
}

        public async Task<bool> EditUserProfileAsync(Guid userId, EditProfileDto editProfileDto)
        {
            // Buscar o usuário
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Verificando a senha atual
            if (!_passwordHasher.VerifyPassword(editProfileDto.CurrentPassword, user.UserPassword))
            {
                return false; // Senha atual incorreta
            }

            // Atualizando a senha, se houver uma nova
            if (!string.IsNullOrEmpty(editProfileDto.NewPassword))
            {
                user.UserPassword = _passwordHasher.HashPassword(editProfileDto.NewPassword);
            }

            // Atualizando o perfil no banco de dados
            var userUpdateResult = await _userRepository.UpdateAsync(user);

            // Atualizando as informações do Customer
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer != null)
            {
                customer.CustomerName = editProfileDto.Name;
                customer.CustomerEmail = editProfileDto.Email;

                var customerUpdateResult = await _customerRepository.UpdateAsync(customer);
                return userUpdateResult && customerUpdateResult;
            }

            return false;
        }
    }
}
