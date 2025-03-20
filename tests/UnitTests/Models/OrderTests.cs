using System;
using Domain.Models;
using Xunit;

namespace UnitTests.Models
{
    public class OrderTests
    {
        [Fact]
        public void OrderDateNaoPodeSerNula()
        {
            // Arrange
            var order = new Order();
              
            //Act
            var Date = order.OrderDate;

            //Assert
            Assert.NotNull(Date);
        }

        [Fact]
        public void OrderItemNaoPodeSerNula()
        {
            // Arrange
            var order = new Order { OrderItems = new List<OrderItem>() };

            // Act
            var orderItems = order.OrderItems;

            // Assert: A lista não pode ser nula, mesmo que esteja vazia
            Assert.NotNull(orderItems);
        }
    }
}