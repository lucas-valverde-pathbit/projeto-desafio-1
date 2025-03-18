using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services;
using Domain.Models;
using Infrastructure.Data;
using System.Linq;

namespace UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly ProductService _productService;
        private readonly Mock<DbSet<Product>> _mockDbSet;

        public ProductServiceTests()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _mockHttpClient = new Mock<HttpClient>();
            _mockDbSet = new Mock<DbSet<Product>>();

            _mockDbContext.Setup(m => m.Products).Returns(_mockDbSet.Object);
            _productService = new ProductService(_mockDbContext.Object, _mockHttpClient.Object);
        }

        [Fact]
        public async Task GetByName_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var productName = "Product A";
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = productName,
                ProductDescription = "Description",
                ProductPrice = 10.99m,
                ProductStockQuantity = 100
            };

            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Product, bool>>(), default))
                      .ReturnsAsync(product);

            // Act
            var result = await _productService.GetByName(productName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productName, result.ProductName);
        }

        [Fact]
        public async Task GetByName_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productName = "NonExistingProduct";

            _mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Product, bool>>(), default))
                      .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetByName(productName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProduct_ShouldCreateProductSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductDescription = "New Description",
                ProductPrice = 20.99m,
                ProductStockQuantity = 50
            };

            _mockDbSet.Setup(m => m.AddAsync(It.IsAny<Product>(), default))
                      .Returns(Task.CompletedTask);

            _mockDbContext.Setup(m => m.SaveChangesAsync(default))
                          .ReturnsAsync(1);

            // Act
            var result = await _productService.CreateProduct(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Product", result.ProductName);
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateProductSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductPrice = 25.99m,
                ProductStockQuantity = 60
            };

            var existingProduct = new Product
            {
                Id = product.Id,
                ProductName = "Old Product",
                ProductDescription = "Old Description",
                ProductPrice = 15.99m,
                ProductStockQuantity = 40
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(existingProduct);

            _mockDbContext.Setup(m => m.SaveChangesAsync(default))
                          .ReturnsAsync(1);

            // Act
            var result = await _productService.UpdateProduct(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product", result.ProductName);
            Assert.Equal("Updated Description", result.ProductDescription);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Non Existing Product",
                ProductDescription = "Description",
                ProductPrice = 15.99m,
                ProductStockQuantity = 10
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.UpdateProduct(product);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProduct_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Product to Delete",
                ProductDescription = "Description",
                ProductPrice = 30.00m,
                ProductStockQuantity = 30
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            _mockDbContext.Setup(m => m.SaveChangesAsync(default))
                          .ReturnsAsync(1);

            // Act
            await _productService.DeleteProduct(productId);

            // Assert
            _mockDbContext.Verify(m => m.Products.Remove(It.IsAny<Product>()), Times.Once);
            _mockDbContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_ShouldNotDelete_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync((Product?)null);

            // Act
            await _productService.DeleteProduct(productId);

            // Assert
            _mockDbContext.Verify(m => m.Products.Remove(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CheckStockAvailability_ShouldReturnTrue_WhenStockIsSufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var quantity = 5;
            var product = new Product
            {
                Id = productId,
                ProductName = "Product A",
                ProductStockQuantity = 10
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            // Act
            var result = await _productService.CheckStockAvailability(productId, quantity);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckStockAvailability_ShouldReturnFalse_WhenStockIsInsufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var quantity = 15;
            var product = new Product
            {
                Id = productId,
                ProductName = "Product B",
                ProductStockQuantity = 10
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            // Act
            var result = await _productService.CheckStockAvailability(productId, quantity);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateProductStock_ShouldReturnTrue_WhenStockIsValid()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var quantity = 5;
            var product = new Product
            {
                Id = productId,
                ProductName = "Valid Product",
                ProductStockQuantity = 10
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            // Act
            var result = await _productService.ValidateProductStock(productId, quantity);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateProductStock_ShouldReturnFalse_WhenStockIsInvalid()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var quantity = 15;
            var product = new Product
            {
                Id = productId,
                ProductName = "Invalid Product",
                ProductStockQuantity = 10
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            // Act
            var result = await _productService.ValidateProductStock(productId, quantity);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateDeliveryAddress_ShouldReturnTrue_WhenAddressIsValid()
        {
            // Arrange
            var deliveryAddress = "12345678";
            _mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>()))
                           .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            // Act
            var result = await _productService.ValidateDeliveryAddress(deliveryAddress);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateDeliveryAddress_ShouldReturnFalse_WhenAddressIsInvalid()
        {
            // Arrange
            var deliveryAddress = "12345678";
            _mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>()))
                           .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest });

            // Act
            var result = await _productService.ValidateDeliveryAddress(deliveryAddress);

            // Assert
            Assert.False(result);
        }
    }
}
