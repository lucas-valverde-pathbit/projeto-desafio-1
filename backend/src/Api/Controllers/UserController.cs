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
using System;

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
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            Console.WriteLine($"Tentativa de login para: {request.LoginEmail}");

            // Recupera o usuário pelo email
            var user = await _service.GetByEmail(request.LoginEmail);

            // Se o usuário não existir ou não for encontrado
            if (user == null)
            {
                Console.WriteLine("Autenticação falhou - usuário não encontrado ou credenciais inválidas");

                // Criando o DTO de erro
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao fazer login. Verifique suas credenciais.",
                    StatusCode = 401,
                    Details = "Usuário não encontrado com esse e-mail."
                };

                return Unauthorized(errorResponse);
            }

            // Verifica se a senha fornecida corresponde ao hash armazenado
            var isPasswordValid = _passwordHasher.VerifyPassword(request.LoginPassword, user.UserPassword);

            // Se a senha for inválida
            if (!isPasswordValid)
            {
                Console.WriteLine("Autenticação falhou - senha inválida");

                // Criando o DTO de erro
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao fazer login. Verifique suas credenciais.",
                    StatusCode = 401,
                    Details = "Senha inválida fornecida."
                };

                return Unauthorized(errorResponse);
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
        public async Task<IActionResult> Signup([FromBody] SignupRequestDTO request)
        {
            // Verifica se já existe um usuário com o email informado
            var existingUser = await _service.GetByEmail(request.SignupEmail);

            if (existingUser != null)
            {
                // Criando o DTO de erro
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Email já cadastrado.",
                    StatusCode = 400,
                    Details = "O e-mail fornecido já está registrado em nossa base de dados."
                };

                return BadRequest(errorResponse);
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

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] EditProfileDto editProfileDto)
        {
            try
            {
                var user = await _service.GetById(userId);

                if (user == null)
                {
                    // Criando o DTO de erro
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "Usuário não encontrado.",
                        StatusCode = 404,
                        Details = "O usuário com o ID fornecido não foi encontrado."
                    };

                    return NotFound(errorResponse);
                }

                if (!_passwordHasher.VerifyPassword(editProfileDto.CurrentPassword, user.UserPassword))
                {
                    // Criando o DTO de erro
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "Senha atual inválida.",
                        StatusCode = 400,
                        Details = "A senha atual fornecida está incorreta."
                    };

                    return BadRequest(errorResponse);
                }

                if (!string.IsNullOrEmpty(editProfileDto.NewPassword))
                {
                    user.UserPassword = _passwordHasher.HashPassword(editProfileDto.NewPassword);
                    await _service.UpdateUserAsync(userId, editProfileDto, user.Role);
                }

                return Ok(new { message = "Perfil atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                // Criando o DTO de erro
                var errorResponse = new ErrorResponseDTO
                {
                    Message = ex.Message,
                    StatusCode = 400,
                    Details = "Houve um erro ao tentar atualizar o perfil."
                };

                return BadRequest(errorResponse);
            }
        }
    }
}
