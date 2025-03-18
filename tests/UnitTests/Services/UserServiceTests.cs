using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using Infrastructure.Services;
using Domain.Models;
using Domain.DTOs;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockDbContext = new Mock<AppDbContext>();
            _userService = new UserService(_mockDbContext.Object, _mockUserRepository.Object, _mockCustomerRepository.Object, _mockPasswordHasher.Object);
        }

        [Fact]
        public async Task Authenticate_ShouldReturnUser_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "test@example.com";
            var password = "password123";
            var hashedPassword = "hashedpassword123";

            var user = new User
            {
                UserEmail = username,
                UserPassword = hashedPassword,
                FailedLoginAttempts = 0,
                LockoutEnd = null
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(password, hashedPassword)).Returns(true);

            // Act
            var result = await _userService.Authenticate(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.UserEmail);
        }

        [Fact]
        public async Task Authenticate_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "test@example.com";
            var password = "wrongpassword";
            var hashedPassword = "hashedpassword123";

            var user = new User
            {
                UserEmail = username,
                UserPassword = hashedPassword,
                FailedLoginAttempts = 0,
                LockoutEnd = null
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(password, hashedPassword)).Returns(false);

            // Act
            var result = await _userService.Authenticate(username, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_ShouldLockUser_WhenFailedLoginAttemptsExceedLimit()
        {
            // Arrange
            var username = "test@example.com";
            var password = "wrongpassword";
            var hashedPassword = "hashedpassword123";

            var user = new User
            {
                UserEmail = username,
                UserPassword = hashedPassword,
                FailedLoginAttempts = 4,  // Just 1 attempt away from lockout
                LockoutEnd = null
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(password, hashedPassword)).Returns(false);
            
            // Act
            var result = await _userService.Authenticate(username, password);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldAddUserSuccessfully()
        {
            // Arrange
            var user = new User
            {
                UserEmail = "newuser@example.com",
                UserPassword = "password123",
                UserName = "New User",
                Role = UserRole.CLIENTE
            };

            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _userService.Create(user);

            // Assert
            _mockUserRepository.Verify(repo => repo.AddAsync(user), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var editProfileDto = new EditProfileDto
            {
                Name = "Updated User",
                Email = "updateduser@example.com",
                NewPassword = "newpassword123"
            };

            var user = new User
            {
                Id = userId,
                UserEmail = "olduser@example.com",
                UserName = "Old User",
                UserPassword = "oldpassword123",
                Role = UserRole.CLIENTE
            };

            var customer = new Customer
            {
                UserId = userId,
                CustomerName = "Old User",
                CustomerEmail = "olduser@example.com"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("hashedNewPassword123");
            _mockCustomerRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(customer);
            _mockUserRepository.Setup(repo => repo.UpdateAsync(user)).ReturnsAsync(true);
            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(customer)).ReturnsAsync(true);

            // Act
            await _userService.UpdateUserAsync(userId, editProfileDto, UserRole.CLIENTE);

            // Assert
            Assert.Equal(editProfileDto.Name, user.UserName);
            Assert.Equal(editProfileDto.Email, user.UserEmail);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(customer), Times.Once);
        }

        [Fact]
        public async Task EditUserProfileAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var editProfileDto = new EditProfileDto
            {
                Name = "Updated User",
                Email = "updateduser@example.com",
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword123"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.EditUserProfileAsync(userId, editProfileDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EditUserProfileAsync_ShouldReturnFalse_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var editProfileDto = new EditProfileDto
            {
                Name = "Updated User",
                Email = "updateduser@example.com",
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword123"
            };

            var user = new User
            {
                Id = userId,
                UserPassword = "correctpassword"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Act
            var result = await _userService.EditUserProfileAsync(userId, editProfileDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EditUserProfileAsync_ShouldReturnTrue_WhenProfileIsUpdatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var editProfileDto = new EditProfileDto
            {
                Name = "Updated User",
                Email = "updateduser@example.com",
                CurrentPassword = "correctpassword",
                NewPassword = "newpassword123"
            };

            var user = new User
            {
                Id = userId,
                UserEmail = "olduser@example.com",
                UserPassword = "correctpassword",
                UserName = "Old User"
            };

            var customer = new Customer
            {
                UserId = userId,
                CustomerName = "Old User",
                CustomerEmail = "olduser@example.com"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockPasswordHasher.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("newHashedPassword123");
            _mockCustomerRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(customer);
            _mockUserRepository.Setup(repo => repo.UpdateAsync(user)).ReturnsAsync(true);
            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(customer)).ReturnsAsync(true);

            // Act
            var result = await _userService.EditUserProfileAsync(userId, editProfileDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated User", user.UserName);
            Assert.Equal("updateduser@example.com", user.UserEmail);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(customer), Times.Once);
        }
    }
}
