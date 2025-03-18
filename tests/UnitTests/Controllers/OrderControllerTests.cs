using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Controllers;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq; // Adicionando a diretiva Moq
using Microsoft.Extensions.Logging; // Adicionando a diretiva para ILogger

namespace UnitTests.Controllers
{
    public class OrderControllerTests
    {
        private readonly OrderController _controller;
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<IProductService> _mockProductService; // Adicionando o mock para IProductService
        private readonly Mock<ICustomerService> _mockCustomerService; // Adicionando o mock para ICustomerService
        private readonly Mock<ILogger<OrderController>> _mockLogger; // Adicionando o mock para ILogger

        public OrderControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockProductService = new Mock<IProductService>(); // Inicializando o mock para IProductService
            _mockCustomerService = new Mock<ICustomerService>(); // Inicializando o mock para ICustomerService
            _mockLogger = new Mock<ILogger<OrderController>>(); // Inicializando o mock para ILogger
            _controller = new OrderController(_mockOrderService.Object, _mockProductService.Object, _mockCustomerService.Object, null, _mockLogger.Object); // Passando os mocks para o construtor
        }

        [Fact]
        public async Task CreateOrder_ReturnsOk_WhenOrderIsCreated()
        {
            // Arrange
            var customerId = Guid.NewGuid(); // Exemplo de CustomerId
            var productId = Guid.NewGuid(); // Exemplo de ProductId
            var quantity = 1; // Exemplo de quantidade
            var deliveryAddress = "123 Main St"; // Exemplo de endereço de entrega

            _mockOrderService.Setup(service => service.CreateOrder(customerId, productId, quantity, deliveryAddress))
                .ReturnsAsync(new Order()); // Assuming Order is a valid return type

            // Act
            var result = await _controller.CreateOrder(new OrderRequestDTO
            {
                CustomerId = customerId,
                DeliveryAddress = deliveryAddress,
                DeliveryZipCode = "12345-678",
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = productId, Quantity = quantity, Price = 100.00M } },
                TotalAmount = 100.00M
            });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Order>(okResult.Value);
        }

        [Fact]
        public async Task CreateOrder_ReturnsBadRequest_WhenOrderRequestIsInvalid()
        {
            // Arrange
            var customerId = Guid.NewGuid(); // Exemplo de CustomerId
            var productId = Guid.NewGuid(); // Exemplo de ProductId
            var quantity = 1; // Exemplo de quantidade
            var deliveryAddress = "123 Main St"; // Exemplo de endereço de entrega

            _mockOrderService.Setup(service => service.CreateOrder(customerId, productId, quantity, deliveryAddress))
                .ThrowsAsync(new Exception("Invalid order request"));

            // Act
            var result = await _controller.CreateOrder(new OrderRequestDTO
            {
                CustomerId = customerId,
                DeliveryAddress = deliveryAddress,
                DeliveryZipCode = "12345-678",
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = productId, Quantity = quantity, Price = 100.00M } },
                TotalAmount = 100.00M
            });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid order request", ((dynamic)badRequestResult.Value).message);
        }
    }
}
