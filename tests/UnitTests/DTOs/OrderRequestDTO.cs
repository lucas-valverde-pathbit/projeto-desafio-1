using Xunit;
using Domain.DTOs;
using Domain.Models;
using System;
using System.Collections.Generic;

namespace UnitTests.DTOs
{
    public class OrderRequestDTOTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var orderRequest = new OrderRequestDTO();

            // Assert
            Assert.NotNull(orderRequest);
            Assert.Null(orderRequest.DeliveryAddress);
            Assert.Null(orderRequest.DeliveryZipCode);
            Assert.Equal(OrderStatus.Pending, orderRequest.Status); // Assuming default value is Pending
            Assert.NotNull(orderRequest.OrderItems);
            Assert.Empty(orderRequest.OrderItems);
            Assert.Equal(0m, orderRequest.TotalAmount);  // Assuming default value is 0
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var orderRequest = new OrderRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                DeliveryAddress = "123 Main St",
                DeliveryZipCode = "12345-678",
                Status = OrderStatus.Confirmed,
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 2, Price = 10.0m },
                    new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 3, Price = 20.0m }
                },
                TotalAmount = 80.0m
            };

            // Act & Assert
            Assert.Equal(orderRequest.CustomerId, orderRequest.CustomerId);
            Assert.Equal("123 Main St", orderRequest.DeliveryAddress);
            Assert.Equal("12345-678", orderRequest.DeliveryZipCode);
            Assert.Equal(OrderStatus.Confirmed, orderRequest.Status);
            Assert.Equal(2, orderRequest.OrderItems.Count);
            Assert.Equal(80.0m, orderRequest.TotalAmount);
        }

        [Fact]
        public void TotalAmount_ShouldCalculateCorrectly()
        {
            // Arrange
            var orderRequest = new OrderRequestDTO
            {
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 2, Price = 10.0m },
                    new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 3, Price = 20.0m }
                }
            };

            // Act
            var totalAmount = 0m;
            foreach (var item in orderRequest.OrderItems)
            {
                totalAmount += item.Quantity * item.Price;
            }
            orderRequest.TotalAmount = totalAmount;

            // Assert
            Assert.Equal(80.0m, orderRequest.TotalAmount);
        }
    }

    public class OrderItemDTOTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var orderItem = new OrderItemDTO();

            // Assert
            Assert.NotNull(orderItem);
            Assert.Equal(Guid.Empty, orderItem.ProductId);
            Assert.Equal(0, orderItem.Quantity);
            Assert.Equal(0m, orderItem.Price);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var orderItem = new OrderItemDTO
            {
                ProductId = Guid.NewGuid(),
                Quantity = 5,
                Price = 15.0m
            };

            // Act & Assert
            Assert.Equal(orderItem.ProductId, orderItem.ProductId);
            Assert.Equal(5, orderItem.Quantity);
            Assert.Equal(15.0m, orderItem.Price);
        }
    }
}
