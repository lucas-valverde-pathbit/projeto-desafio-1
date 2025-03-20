using System;
using Domain.Models;
using Xunit;

namespace UnitTests.Models
{
    public class CustomerTests
    {
        [Fact]
        public void OrdersPodemSerNulas()
        {
            // Arrange
            var customer = new Customer();
            customer.CustomerName = "Joao";
            customer.CustomerEmail = "joao@joao.com";
            // Act
            customer.Orders = null;

            // Assert
            Assert.Null(customer.Orders);
        }
    }
}
