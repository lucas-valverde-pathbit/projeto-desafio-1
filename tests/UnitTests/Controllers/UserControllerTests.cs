using Xunit;
using Moq;
using Api.Controllers;
using Domain.Models;
using Domain.Services;
using Domain.Repositories; // Adicionando a diretiva using para ICustomerRepository
using Domain.DTOs; // Adicionando a diretiva using para SignupRequest
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _controller = new UserController(_mockUserService.Object, _mockPasswordHasher.Object, _mockCustomerRepository.Object);
        }

        [Fact]
        public async Task CreateUser_ReturnsOk_WhenUserIsCreated()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                UserEmail = "testuser@example.com",
                UserPassword = "Password123",
                Role = UserRole.CLIENTE
            };

            _mockUserService.Setup(service => service.Create(It.IsAny<User>()))
                .Returns(Task.FromResult(user)); // Ajustando para usar Returns corretamente

            // Act
            var result = await _controller.Signup(new SignupRequest
            {
                SignupName = user.UserName,
                SignupEmail = user.UserEmail,
                SignupPassword = user.UserPassword,
                SignupRole = user.Role
            });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Usuário cadastrado com sucesso!", ((dynamic)okResult.Value).message);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenUserIsInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                UserEmail = "testuser@example.com",
                UserPassword = "Password123",
                Role = UserRole.CLIENTE
            };

            _mockUserService.Setup(service => service.Create(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Invalid user request"));

            // Act
            var result = await _controller.Signup(new SignupRequest
            {
                SignupName = user.UserName,
                SignupEmail = user.UserEmail,
                SignupPassword = user.UserPassword,
                SignupRole = user.Role
            });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user request", ((dynamic)badRequestResult.Value).message);
        }
    }
}
