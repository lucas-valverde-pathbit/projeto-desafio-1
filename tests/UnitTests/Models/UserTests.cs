using System;
using Xunit;
using Domain.Models;
using Domain.Services;

namespace UnitTests.Models
{
    public class UserTests
    {
        [Fact]
        public void Should_Create_User_With_Valid_Values()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                UserEmail = "testuser@example.com",
                UserPassword = "SecurePassword123!",
                Role = UserRole.CLIENTE,
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 0
            };

            // Assert
            Assert.NotNull(user);
            Assert.Equal("testuser", user.UserName);
            Assert.Equal("testuser@example.com", user.UserEmail);
            Assert.Equal("SecurePassword123!", user.UserPassword);
            Assert.Equal(UserRole.CLIENTE, user.Role);
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.True(user.CreatedAt <= DateTime.UtcNow); // Ensure the creation date is set correctly
            Assert.Equal(0, user.FailedLoginAttempts);
        }

        [Fact]
        public void Should_Initialize_Empty_User()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(Guid.Empty, user.Id);
            Assert.Null(user.UserName);
            Assert.Null(user.UserEmail);
            Assert.Null(user.UserPassword);
            Assert.Equal(UserRole.CLIENTE, user.Role); // Default value for UserRole is CLIENTE
            Assert.Equal(0, user.FailedLoginAttempts);
            Assert.Null(user.CreatedAt);
            Assert.Null(user.UpdatedAt);
            Assert.Null(user.LockoutEnd);
        }

        [Fact]
        public void Should_Set_Values_To_Properties()
        {
            // Arrange
            var user = new User();

            // Act
            user.Id = Guid.NewGuid();
            user.UserName = "updateduser";
            user.UserEmail = "updateduser@example.com";
            user.UserPassword = "UpdatedPassword123!";
            user.Role = UserRole.ADMINISTRADOR;
            user.CreatedAt = DateTime.UtcNow;
            user.FailedLoginAttempts = 3;
            user.UpdatedAt = DateTime.UtcNow.AddHours(1);
            user.LockoutEnd = DateTime.UtcNow.AddHours(2);

            // Assert
            Assert.Equal("updateduser", user.UserName);
            Assert.Equal("updateduser@example.com", user.UserEmail);
            Assert.Equal("UpdatedPassword123!", user.UserPassword);
            Assert.Equal(UserRole.ADMINISTRADOR, user.Role);
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
            Assert.Equal(3, user.FailedLoginAttempts);
            Assert.NotNull(user.UpdatedAt);
            Assert.True(user.LockoutEnd > DateTime.UtcNow);
        }

        [Fact]
        public void Should_Increment_FailedLoginAttempts_When_Login_Fails()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                UserEmail = "testuser@example.com",
                UserPassword = "SecurePassword123!",
                Role = UserRole.CLIENTE,
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 0
            };

            // Act
            user.FailedLoginAttempts++;

            // Assert
            Assert.Equal(1, user.FailedLoginAttempts);
        }

        [Fact]
        public void Should_Reset_FailedLoginAttempts_When_Login_Succeeds()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                UserEmail = "testuser@example.com",
                UserPassword = "SecurePassword123!",
                Role = UserRole.CLIENTE,
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 3
            };

            // Act
            user.FailedLoginAttempts = 0;

            // Assert
            Assert.Equal(0, user.FailedLoginAttempts);
        }

        [Fact]
        public void Should_Update_LockoutEnd_When_LockedOut()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "lockeduser",
                UserEmail = "lockeduser@example.com",
                UserPassword = "Password123!",
                Role = UserRole.CLIENTE,
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 5
            };

            // Act
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(30); // Lockout for 30 minutes

            // Assert
            Assert.NotNull(user.LockoutEnd);
            Assert.True(user.LockoutEnd > DateTime.UtcNow); // Ensure the lockout is set for the future
        }

        [Fact]
        public void Should_Default_Role_To_CLIENTE_When_Not_Set()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                UserEmail = "testuser@example.com",
                UserPassword = "SecurePassword123!",
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 0
            };

            // Assert
            Assert.Equal(UserRole.CLIENTE, user.Role); // Default role should be CLIENTE
        }
    }
}
