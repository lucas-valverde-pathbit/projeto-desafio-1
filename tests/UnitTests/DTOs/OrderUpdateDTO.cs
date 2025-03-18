using Xunit;
using Domain.DTOs;
using System;

namespace UnitTests.DTOs
{
    public class OrderUpdateDTOTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var orderUpdate = new OrderUpdateDTO();

            // Assert
            Assert.NotNull(orderUpdate);
            Assert.Null(orderUpdate.DeliveryAddress);
            Assert.Null(orderUpdate.Status);
            Assert.Equal(0, orderUpdate.AddressId);
            Assert.Null(orderUpdate.ZipCode);
            Assert.Null(orderUpdate.ZipCodeFormatted);
            Assert.Null(orderUpdate.AddressType);
            Assert.Null(orderUpdate.AddressName);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var orderUpdate = new OrderUpdateDTO
            {
                DeliveryAddress = "123 Main St",
                Status = "Confirmed",
                AddressId = 101,
                ZipCode = "12345-678",
                ZipCodeFormatted = "12345-678",
                AddressType = "Residential",
                AddressName = "Home"
            };

            // Act & Assert
            Assert.Equal("123 Main St", orderUpdate.DeliveryAddress);
            Assert.Equal("Confirmed", orderUpdate.Status);
            Assert.Equal(101, orderUpdate.AddressId);
            Assert.Equal("12345-678", orderUpdate.ZipCode);
            Assert.Equal("12345-678", orderUpdate.ZipCodeFormatted);
            Assert.Equal("Residential", orderUpdate.AddressType);
            Assert.Equal("Home", orderUpdate.AddressName);
        }

        [Fact]
        public void ShouldAllowNullForOptionalProperties()
        {
            // Arrange
            var orderUpdate = new OrderUpdateDTO
            {
                DeliveryAddress = null,
                Status = null,
                ZipCode = null,
                ZipCodeFormatted = null,
                AddressType = null,
                AddressName = null
            };

            // Act & Assert
            Assert.Null(orderUpdate.DeliveryAddress);
            Assert.Null(orderUpdate.Status);
            Assert.Null(orderUpdate.ZipCode);
            Assert.Null(orderUpdate.ZipCodeFormatted);
            Assert.Null(orderUpdate.AddressType);
            Assert.Null(orderUpdate.AddressName);
        }

        [Fact]
        public void AddressId_ShouldBeSetCorrectly()
        {
            // Arrange
            var orderUpdate = new OrderUpdateDTO
            {
                AddressId = 123
            };

            // Act & Assert
            Assert.Equal(123, orderUpdate.AddressId);
        }
    }
}
