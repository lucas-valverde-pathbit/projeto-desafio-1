using System;
using Xunit;
using Domain.Models;

namespace UnitTests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Should_Create_Product_With_Valid_Values()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductDescription = "Test Description",
                ProductPrice = 99.99m,
                ProductStockQuantity = 50
            };

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Test Product", product.ProductName);
            Assert.Equal("Test Description", product.ProductDescription);
            Assert.Equal(99.99m, product.ProductPrice);
            Assert.Equal(50, product.ProductStockQuantity);
        }

        [Fact]
        public void Should_Initialize_Empty_Product()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.NotNull(product);
            Assert.Equal(Guid.Empty, product.Id);
            Assert.Null(product.ProductName);
            Assert.Null(product.ProductDescription);
            Assert.Equal(0m, product.ProductPrice);
            Assert.Equal(0, product.ProductStockQuantity);
        }

        [Fact]
        public void Should_Set_Values_To_Properties()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = Guid.NewGuid();
            product.ProductName = "Updated Product";
            product.ProductDescription = "Updated Description";
            product.ProductPrice = 150.0m;
            product.ProductStockQuantity = 30;

            // Assert
            Assert.Equal("Updated Product", product.ProductName);
            Assert.Equal("Updated Description", product.ProductDescription);
            Assert.Equal(150.0m, product.ProductPrice);
            Assert.Equal(30, product.ProductStockQuantity);
        }

        [Fact]
        public void Should_Throw_Exception_When_Setting_Negative_Stock_Quantity()
        {
            // Arrange
            var product = new Product();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => product.ProductStockQuantity = -1);
            Assert.Equal("Stock quantity cannot be negative.", exception.Message);
        }

        [Fact]
        public void Should_Throw_Exception_When_Setting_Negative_Price()
        {
            // Arrange
            var product = new Product();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => product.ProductPrice = -10.0m);
            Assert.Equal("Price cannot be negative.", exception.Message);
        }
    }
}
