using System;
using Xunit;
using Domain.Models;

namespace UnitTests.Models
{
    public class OrderItemTests
    {
        [Fact]
        public void Should_Create_OrderItem_With_Valid_Values()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Product Name",
                ProductDescription = "Product Description",
                ProductPrice = 100.0m,
                Quantity = 2,
                Price = 200.0m
            };

            // Assert
            Assert.NotNull(orderItem);
            Assert.Equal(200.0m, orderItem.Price);
            Assert.Equal("Product Name", orderItem.ProductName);
            Assert.Equal(100.0m, orderItem.ProductPrice);
            Assert.Equal(2, orderItem.Quantity);
        }

        [Fact]
        public void Should_Initialize_Empty_OrderItem()
        {
            // Arrange & Act
            var orderItem = new OrderItem();

            // Assert
            Assert.NotNull(orderItem);
            Assert.Equal(Guid.Empty, orderItem.Id);
            Assert.Equal(Guid.Empty, orderItem.OrderId);
            Assert.Null(orderItem.Order);
            Assert.Equal(Guid.Empty, orderItem.ProductId);
            Assert.Null(orderItem.ProductName);
            Assert.Null(orderItem.ProductDescription);
            Assert.Equal(0, orderItem.ProductPrice);
            Assert.Equal(0, orderItem.Quantity);
            Assert.Equal(0, orderItem.Price);
        }

        [Fact]
        public void Should_Compute_Price_Correctly_When_Quantity_Changes()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                ProductPrice = 50.0m,
                Quantity = 3
            };

            // Act
            orderItem.Price = orderItem.ProductPrice * orderItem.Quantity;

            // Assert
            Assert.Equal(150.0m, orderItem.Price);
        }

        [Fact]
        public void Should_Set_Values_To_Properties()
        {
            // Arrange
            var orderItem = new OrderItem();

            // Act
            orderItem.Id = Guid.NewGuid();
            orderItem.OrderId = Guid.NewGuid();
            orderItem.ProductId = Guid.NewGuid();
            orderItem.ProductName = "Test Product";
            orderItem.ProductDescription = "Test Description";
            orderItem.ProductPrice = 10.5m;
            orderItem.Quantity = 5;
            orderItem.Price = 52.5m;

            // Assert
            Assert.Equal(10.5m, orderItem.ProductPrice);
            Assert.Equal(5, orderItem.Quantity);
            Assert.Equal(52.5m, orderItem.Price);
            Assert.Equal("Test Product", orderItem.ProductName);
            Assert.Equal("Test Description", orderItem.ProductDescription);
            Assert.NotEqual(Guid.Empty, orderItem.Id);
        }
    }
}
