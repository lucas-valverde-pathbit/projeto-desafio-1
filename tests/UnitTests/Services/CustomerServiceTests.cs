using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Infrastructure.Services;
using Domain.Models;
using Infrastructure.Data;
using Domain.Services;

namespace UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly Mock<DbSet<Customer>> _mockDbSet;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _mockDbSet = new Mock<DbSet<Customer>>();
            _mockDbContext.Setup(c => c.Customers).Returns(_mockDbSet.Object);
            _customerService = new CustomerService(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetByEmail_ShouldReturnCustomer_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            var customer = new Customer
            {
                CustomerEmail = email,
                UserId = Guid.NewGuid(),
                // Configure outros campos se necessário
            };

            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Customer, bool>>(), default))
                      .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.CustomerEmail);
        }

        [Fact]
        public async Task GetByEmail_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Customer, bool>>(), default))
                      .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.GetByEmail(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserId_ShouldReturnCustomer_WhenUserIdExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customer = new Customer
            {
                CustomerEmail = "test@example.com",
                UserId = userId,
                User = new User { Id = userId },
                // Configure outros campos se necessário
            };

            _mockDbSet.Setup(m => m.Include(It.IsAny<Func<Customer, object>>()))
                      .Returns(_mockDbSet.Object);
            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Customer, bool>>(), default))
                      .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task GetByUserId_ShouldReturnNull_WhenUserIdDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockDbSet.Setup(m => m.Include(It.IsAny<Func<Customer, object>>()))
                      .Returns(_mockDbSet.Object);
            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Customer, bool>>(), default))
                      .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.GetByUserId(userId);

            // Assert
            Assert.Null(result);
        }
    }
}
