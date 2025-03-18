using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using Domain.Repositories;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Api.Controllers
{
        [ApiController]
    [Route("api/users")]
    public class UserController : BaseController<User, IUserService>
    {
        // Injeção de dependências para o PasswordHasher e CustomerRepository
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICustomerRepository _customerRepository;

        public UserController(IUserService service, IPasswordHasher passwordHasher, ICustomerRepository customerRepository) 
            : base(service)
        {
            _passwordHasher = passwordHasher;
            _customerRepository = customerRepository;
        }

       [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    Console.WriteLine($"Tentativa de login para: {request.LoginEmail}");
    
    // Recupera o usuário pelo email
    var user = await _service.GetByEmail(request.LoginEmail);
    
    // Se o usuário não existir ou não for encontrado
    if (user == null)
    {
        Console.WriteLine("Autenticação falhou - usuário não encontrado ou credenciais inválidas");
        return Unauthorized(new { message = "Erro ao fazer login. Verifique suas credenciais." });
    }

    // Verifica se a senha fornecida corresponde ao hash armazenado
    var isPasswordValid = _passwordHasher.VerifyPassword(request.LoginPassword, user.UserPassword);
    
    // Se a senha for inválida
    if (!isPasswordValid)
    {
        Console.WriteLine("Autenticação falhou - senha inválida");
        return Unauthorized(new { message = "Erro ao fazer login. Verifique suas credenciais." });
    }

    Console.WriteLine($"Login bem-sucedido para: {user.UserEmail}");

    // Geração do token JWT
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("your_secure_long_key_here_256_bits"); // Alterar para chave segura real

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.UserEmail),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Ok(new { 
        user.Id,
        user.UserName,
        user.UserEmail,
        Token = tokenString
    });
}
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
         // Verifica se já existe um usuário com o email informado
         var existingUser = await _service.GetByEmail(request.SignupEmail);

         if (existingUser != null)
         {
                return BadRequest(new { message = "Email já cadastrado." });
        }

         // Cria um novo usuário
         var newUser = new User
         {
                UserName = request.SignupName,
                UserEmail = request.SignupEmail,
                UserPassword = _passwordHasher.HashPassword(request.SignupPassword),
                Role = request.SignupRole
         };

            // Criação do novo cliente se a role for CLIENTE (enum 1)
         if (request.SignupRole == UserRole.CLIENTE)
         {
              var newCustomer = new Customer
                {
                 CustomerName = request.SignupName,
                 CustomerEmail = request.SignupEmail,

                };

   
             await _customerRepository.AddAsync(newCustomer); // Certifique-se de ter um repositório de clientes
         }

         // Salva o novo usuário no banco de dados
         await _service.Create(newUser); // Assumindo que _service.Create cria o usuário e o salva no banco

          // Caso a role seja CLIENTE, você pode associar o UserId ao Customer
          if (request.SignupRole == UserRole.CLIENTE)
          {
                var customer = await _customerRepository.GetByEmailAsync(request.SignupEmail);
                if (customer != null)
                {
                   customer.UserId = newUser.Id; // Associando o UserId ao Customer
                    await _customerRepository.UpdateAsync(customer); // Atualizando o cliente no banco de dados
               }
          }

          
            return Ok(new { message = "Usuário e Cliente cadastrados com sucesso!" });
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

         [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] EditProfileDto editProfileDto)
        {
            try
            {
  
                UserRole userRole = UserRole.CLIENTE; 
                await _service.UpdateUserAsync(userId, editProfileDto, userRole);

                return Ok(new { message = "Perfil atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        public class LoginRequest
        {
            public required string LoginEmail { get; set; }
            public required string LoginPassword { get; set; }
        }

        public class SignupRequest
        {
            public required string SignupName { get; set; }
            public required string SignupEmail { get; set; }
            public required string SignupPassword { get; set; }
            public required UserRole SignupRole { get; set; }
        }
    }
}
