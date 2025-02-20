using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _service;
        private readonly ICustomerService _customerService;
        public UserController(IUserService service, ICustomerService customerService, IConfiguration configuration, ILogger<UserController> logger)
        {
            _service = service;
            _configuration = configuration;
            _logger = logger;
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
             var users = await _service.GetAll();
                if (users == null || !users.Any())
                return NotFound("Nenhum usuário encontrado.");

             return Ok(users);
        }
        // Endpoint de Cadastro de Usuário
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserSignupRequest request)
        {
          _logger.LogInformation("Recebida requisição de cadastro: {@Request}", request);

          // Validações
          if (string.IsNullOrEmpty(request.SignupPassword))
                return BadRequest("Senha é obrigatória");

         if (string.IsNullOrEmpty(request.SignupName) || string.IsNullOrEmpty(request.SignupEmail))
                return BadRequest("Nome e email são obrigatórios");

         if (!Enum.TryParse(request.SignupRole, true, out UserRole signupRole))
         {
                _logger.LogWarning("Tipo de usuário inválido recebido: {Role}", request.SignupRole);
             return BadRequest($"Tipo de usuário inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}");
         }

         // Criação do novo usuário
          var user = new User
         {
              UserName = request.SignupName,
               UserEmail = request.SignupEmail,
               UserPassword = ComputeSha256Hash(request.SignupPassword), // Hash da senha
              Role = signupRole
         };

         // Tenta adicionar o usuário ao banco de dados
         var createdUser = await _service.Add(user);

         // Se o tipo de usuário for "CLIENTE", cria um registro na tabela Customer
         if (signupRole == UserRole.CLIENTE)
         {
              // Aqui, você pode adicionar campos específicos para o cliente, por exemplo, endereço ou telefone
              var customer = new Customer
             {
                    UserId = createdUser.Id,  // Aqui você associa o Customer ao User criado
                    CustomerEmail = request.SignupEmail,
                    CustomerName = request.SignupName,
                 // Se tiver mais informações a serem passadas, adicione-as aqui
              };

              // Insere o cliente na tabela Customer
              await _customerService.Add(customer); // Supondo que você tenha um serviço para manipular clientes
            }

         return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }



        // Endpoint de Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.LoginEmail) || string.IsNullOrEmpty(request.LoginPassword))
                return BadRequest("Email e senha são obrigatórios");

            // Autenticação do usuário
            var user = await _service.Authenticate(request.LoginEmail, request.LoginPassword);

            if (user == null)
                return Unauthorized("Email ou senha inválidos");

            // Geração do Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }

        // Método auxiliar para calcular o hash da senha
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

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { status = "API is running" });
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
    }

    // Modelo de Requisição para Cadastro
    public class UserSignupRequest
    {
        public string SignupName { get; set; }
        public string SignupEmail { get; set; }
        public string SignupPassword { get; set; }
        public string SignupRole { get; set; }
    }

    // Modelo de Requisição para Login
    public class UserLoginRequest
    {
        public string LoginEmail { get; set; }
        public string LoginPassword { get; set; }
    }
}
