using Moq;
using Xunit;
using Api.Controllers;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _userController = new UserController(_userServiceMock.Object, _passwordHasherMock.Object, _customerRepositoryMock.Object);
        }

        // Teste 1: Login com senha correta
        [Fact]
        public async Task LoginRetornaOkQuandoSejaEstaCorreta()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO { LoginEmail = "test@example.com", LoginPassword = "correctpassword" };
            var user = new User { Id = Guid.NewGuid(), UserName="usuarioteste", UserEmail = "test@example.com", UserPassword = "hashed_password" };

            _userServiceMock.Setup(service => service.GetByEmail(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            // Act
            var result = await _userController.Login(loginRequest);

            // Assert
            Assert.IsType<OkObjectResult>(result); // Verifica se o resultado é Ok
        }

        // Teste 2: Login com senha incorreta
        [Fact]
        public async Task LoginRetornaUnauthorizedQuandoSenhaEstaIncorreta()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO { LoginEmail = "test@example.com", LoginPassword = "wrongpassword" };
            var user = new User { Id = Guid.NewGuid(), UserName = "UsuarioTeste", UserEmail = "test@example.com", UserPassword = "hashed_password" };

            _userServiceMock.Setup(service => service.GetByEmail(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Act
            var result = await _userController.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result); // Verifica se o resultado é Unauthorized
        }

        // Teste 3: Cadastro de usuário com email já registrado
        [Fact]
        public async Task SignupRetornaBadRequestQuandoEmailParaCadastroJaExiste()
        {
            // Arrange
            var signupRequest = new SignupRequestDTO { SignupName = "Test User", SignupEmail = "test@example.com", SignupPassword = "password123", SignupRole = UserRole.CLIENTE };
            var existingUser = new User {  Id = Guid.NewGuid(), UserName="usuarioteste", UserEmail = "test@example.com", UserPassword = "hashed_password" };

            _userServiceMock.Setup(service => service.GetByEmail(It.IsAny<string>())).ReturnsAsync(existingUser);

            // Act
            var result = await _userController.Signup(signupRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
