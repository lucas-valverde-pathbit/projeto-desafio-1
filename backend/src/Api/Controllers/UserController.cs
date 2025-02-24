using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Domain.Services;
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
        public UserController(IUserService service) : base(service) {}

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { status = "API está online e funcionando" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Tentativa de login para: {request.LoginEmail}");
            
            var user = await _service.Authenticate(request.LoginEmail, request.LoginPassword);
            
            if (user == null)
            {
                Console.WriteLine("Autenticação falhou - usuário não encontrado ou credenciais inválidas");
                return Unauthorized(new { message = "Erro ao fazer login. Verifique suas credenciais." });
            }
            
            Console.WriteLine($"Login bem-sucedido para: {user.UserEmail}");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your_secret_key_here"); // Use a secure key in production
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { 
                user.Id,
                user.UserName,
                user.UserEmail,
                user.Role,
                Token = tokenString

            });

        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            var existingUser = await _service.GetByEmail(request.SignupEmail);
            
            if (existingUser != null)
                return Conflict("Email já cadastrado");

            if (!Enum.TryParse<UserRole>(request.SignupRole, out var role))
            {
                return BadRequest("Tipo de usuário inválido. Escolha entre CLIENTE ou ADMINISTRADOR");
            }

            var user = new User
            {
                UserName = request.SignupName,
                UserEmail = request.SignupEmail,
                UserPassword = ComputeSha256Hash(request.SignupPassword),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };


            await _service.Create(user);
            
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
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
            public required string SignupRole { get; set; }
        }


}
