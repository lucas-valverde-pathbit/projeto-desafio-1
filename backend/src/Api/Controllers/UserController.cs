using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
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
        public UserController(IUserService service) : base(service){}
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
            var existingUser = await _service.GetByEmail(request.SignupEmail);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email já cadastrado." });
            }

            var newUser = new User
            {
                UserName = request.SignupName,
                UserEmail = request.SignupEmail,
                UserPassword = ComputeSha256Hash(request.SignupPassword),
                Role = request.SignupRole
            };

            await _service.Create(newUser);

            return Ok(new { message = "Usuário cadastrado com sucesso!" });
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

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(
            Guid userId,
            string userName,
            string userEmail,
            string userPassword,
            UserRole role,
            string? customerName = null,
            string? customerEmail = null)
        {
            try
            {
                await _service.UpdateUserAsync(userId, userName, userEmail, userPassword, role, customerName, customerEmail);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto editProfileDto)
        {
            // Validando a entrada
            if (editProfileDto == null || string.IsNullOrEmpty(editProfileDto.Name) || string.IsNullOrEmpty(editProfileDto.Email))
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var userId = Guid.NewGuid(); // Aqui você deve pegar o ID do usuário autenticado, geralmente do JWT ou sessão
                var result = await _service.EditUserProfileAsync(userId, editProfileDto);

                if (!result)
                {
                    return BadRequest("Erro ao atualizar o perfil.");
                }

                return Ok("Perfil atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
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
