using System;
using Domain.Models;
using Xunit;

namespace UnitTests.Models
{
    public class CustomerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmptyCustomer()
        {
            // Arrange & Act
            var customer = new Customer();

            // Assert
            Assert.NotNull(customer);
            Assert.Equal(Guid.Empty, customer.Id);
            Assert.Null(customer.CustomerName);
            Assert.Null(customer.CustomerEmail);
            Assert.Null(customer.UserId);
            Assert.Null(customer.User);
            Assert.Null(customer.Orders);
        }

        [Fact]
        public void CustomerName_ShouldBeSetAndGet()
        {
            // Arrange
            var customer = new Customer();
            var customerName = "John Doe";

            // Act
            customer.CustomerName = customerName;

            // Assert
            Assert.Equal(customerName, customer.CustomerName);
        }

        [Fact]
        public void CustomerEmail_ShouldBeSetAndGet()
        {
            // Arrange
            var customer = new Customer();
            var customerEmail = "john.doe@example.com";

            // Act
            customer.CustomerEmail = customerEmail;

            // Assert
            Assert.Equal(customerEmail, customer.CustomerEmail);
        }

        [Fact]
        public void UserId_ShouldBeSetAndGet()
        {
            // Arrange
            var customer = new Customer();
            var userId = Guid.NewGuid();

            // Act
            customer.UserId = userId;

            // Assert
            Assert.Equal(userId, customer.UserId);
        }

        [Fact]
        public void Orders_ShouldBeSetAndGet()
        {
            // Arrange
            var customer = new Customer();
            var orders = new List<Order>();

            // Act
            customer.Orders = orders;

            // Assert
            Assert.Equal(orders, customer.Orders);
        }

        [Fact]
        public void ShouldSetAndGetUser()
        {
            // Arrange
            var customer = new Customer();
            var user = new User();

            // Act
            customer.User = user;

            // Assert
            Assert.Equal(user, customer.User);
        }

        [Fact]
        public void CustomerId_ShouldBeSetAndGet()
        {
            // Arrange
            var customer = new Customer();
            var customerId = Guid.NewGuid();

            // Act
            customer.Id = customerId;

            // Assert
            Assert.Equal(customerId, customer.Id);
        }
    }
}
