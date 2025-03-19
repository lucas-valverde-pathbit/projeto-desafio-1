using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Api.Controllers;
using Domain.Services;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Adicionando a diretiva para ILogger

namespace UnitTests.Controllers
{
    public class ProductControllerTests
    {
        private readonly ProductController _controller;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductController>> _mockLogger;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductController(_mockProductService.Object); // Ajustando para usar apenas um argumento
        }

        [Fact]
        public async Task CreateProduct_ReturnsOk_WhenProductIsCreated()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductPrice = 100.00M,
                ProductStockQuantity = 10
            };

            _mockProductService.Setup(service => service.CreateProduct(product))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.CreateProduct(product);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Product>(okResult.Value);
        }

        [Fact]
        public async Task CreateProduct_ReturnsBadRequest_WhenProductIsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductPrice = 100.00M,
                ProductStockQuantity = 10
            };

            _mockProductService.Setup(service => service.CreateProduct(product))
                .ThrowsAsync(new Exception("Invalid product request"));

            // Act
            var result = await _controller.CreateProduct(product);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid product request", ((dynamic)badRequestResult.Value).message);
        }
    }
}
