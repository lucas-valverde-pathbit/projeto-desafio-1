using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Services;
using Domain.Models;
using Infrastructure.Data;
using Domain.Services;
using Domain.DTOs;

namespace UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _mockCustomerService = new Mock<ICustomerService>();
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            _mockHttpClient = new Mock<HttpClient>();

            _orderService = new OrderService(
                _mockDbContext.Object,
                _mockCustomerService.Object,
                _mockProductService.Object,
                _mockLogger.Object,
                _mockHttpClient.Object
            );
        }

        [Fact]
        public async Task CreateOrder_ShouldThrowException_WhenCustomerNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 1;
            var deliveryAddress = "12345678"; // Exemplo de endereço

            _mockCustomerService.Setup(s => s.GetById(customerId)).ReturnsAsync((Customer?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _orderService.CreateOrder(customerId, productId, quantity, deliveryAddress));
            Assert.Equal("Cliente não encontrado ou não autorizado.", exception.Message);
        }

        [Fact]
        public async Task CreateOrder_ShouldThrowException_WhenInvalidAddress()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 1;
            var deliveryAddress = "12345678"; // Exemplo de endereço

            var customer = new Customer { User = new User { Role = UserRole.CLIENTE } };
            _mockCustomerService.Setup(s => s.GetById(customerId)).ReturnsAsync(customer);

            _mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _orderService.CreateOrder(customerId, productId, quantity, deliveryAddress));
            Assert.Equal("Endereço de entrega inválido.", exception.Message);
        }

        [Fact]
        public async Task CreateOrder_ShouldThrowException_WhenProductStockIsInsufficient()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 10;
            var deliveryAddress = "12345678"; // Exemplo de endereço

            var customer = new Customer { User = new User { Role = UserRole.CLIENTE } };
            var product = new Product { ProductStockQuantity = 5 };
            
            _mockCustomerService.Setup(s => s.GetById(customerId)).ReturnsAsync(customer);
            _mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });
            _mockProductService.Setup(s => s.GetById(productId)).ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _orderService.CreateOrder(customerId, productId, quantity, deliveryAddress));
            Assert.Equal($"Estoque insuficiente para o produto {productId}.", exception.Message);
        }

        [Fact]
        public async Task CreateOrder_ShouldCreateOrderSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 2;
            var deliveryAddress = "12345678"; // Exemplo de endereço

            var customer = new Customer { User = new User { Role = UserRole.CLIENTE } };
            var product = new Product { ProductPrice = 100, ProductStockQuantity = 10 };
            var order = new Order
            {
                CustomerId = customerId,
                DeliveryAddress = deliveryAddress,
                OrderItems = new List<OrderItem>()
            };

            var orderItem = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                ProductPrice = product.ProductPrice,
                Order = order
            };

            _mockCustomerService.Setup(s => s.GetById(customerId)).ReturnsAsync(customer);
            _mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });
            _mockProductService.Setup(s => s.GetById(productId)).ReturnsAsync(product);
            _mockProductService.Setup(s => s.GetPriceById(productId)).ReturnsAsync(product.ProductPrice);

            _mockDbContext.Setup(c => c.Orders.AddAsync(It.IsAny<Order>(), default)).Returns(Task.CompletedTask);
            _mockDbContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _orderService.CreateOrder(customerId, productId, quantity, deliveryAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.OrderItems.Count);
            Assert.Equal(200, result.TotalAmount);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnNull_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockDbContext.Setup(c => c.Orders.FindAsync(orderId)).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.UpdateOrderStatus(orderId, (int)OrderStatus.Entregue);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldUpdateStatusSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Status = OrderStatus.Pendente };
            _mockDbContext.Setup(c => c.Orders.FindAsync(orderId)).ReturnsAsync(order);
            _mockDbContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _orderService.UpdateOrderStatus(orderId, (int)OrderStatus.Entregue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Entregue, result.Status);
        }

        [Fact]
        public async Task CalculateTotalPrice_ShouldReturnCorrectTotal()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductPrice = 100, Quantity = 2 },
                    new OrderItem { ProductPrice = 50, Quantity = 1 }
                }
            };

            _mockDbContext.Setup(c => c.Orders.FindAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.CalculateTotalPrice(orderId);

            // Assert
            Assert.Equal(250, result);
        }
    }
}
