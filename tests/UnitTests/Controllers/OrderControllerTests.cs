using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Controllers;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace UnitTests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<ILogger<OrderController>> _loggerMock;
        private readonly AppDbContext _context;

        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _productServiceMock = new Mock<IProductService>();
            _customerServiceMock = new Mock<ICustomerService>();
            _loggerMock = new Mock<ILogger<OrderController>>();

          
            var options = new DbContextOptionsBuilder<AppDbContext>()
                            .UseInMemoryDatabase("TestDb")
                            .Options;
            _context = new AppDbContext(options);

            _controller = new OrderController(
                _orderServiceMock.Object,
                _productServiceMock.Object,
                _customerServiceMock.Object,
                _context,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateOrderComRequestInválidaRetornaBadRequest()
        {
            // Arrange
            var orderRequest = new OrderRequestDTO
            {
                OrderItems = null // Itens de pedido nulos
            };

            // Act
            var result = await _controller.CreateOrder(orderRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(badRequestResult.Value);
            Assert.Equal(400, errorResponse.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var orderRequest = new OrderRequestDTO
            {
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 1 }
                },
                DeliveryAddress = "123 Street",
                DeliveryZipCode = "12345",
                Status = 1,
                TotalAmount = 100.00m
            };

            var customer = new Customer { Id = Guid.NewGuid() };
            _customerServiceMock.Setup(service => service.GetByUserId(It.IsAny<Guid>())).ReturnsAsync(customer);
            var product = new Product { Id = orderRequest.OrderItems[0].ProductId, ProductStockQuantity = 10 };
            _productServiceMock.Setup(service => service.GetById(It.IsAny<Guid>())).ReturnsAsync(product);

            // Act
            var result = await _controller.CreateOrder(orderRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var order = Assert.IsType<Order>(createdResult.Value);
            Assert.Equal(orderRequest.DeliveryAddress, order.DeliveryAddress);
            Assert.Equal(orderRequest.TotalAmount, order.TotalAmount);
        }

        [Fact]
        public async Task GetById_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = await _controller.GetById(orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);
            Assert.Equal(404, errorResponse.StatusCode);
        }

        [Fact]
        public async Task GetById_OrderFound_ReturnsOk()
        {
            Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Test Product", Quantity = 1, ProductPrice = 100.00m }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            Act
            var result = await _controller.GetById(orderId);

            Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orderResult = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(orderId, orderResult.Id);
        }

        [Fact]
        public async Task GetByCustomerId_NoOrdersFound_ReturnsNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Act
            var result = await _controller.GetByCustomerId(customerId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);
            Assert.Equal(404, errorResponse.StatusCode);
        }

        [Fact]
        public async Task GetByCustomerId_OrdersFound_ReturnsOk()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = new Order { Id = Guid.NewGuid(), CustomerId = customerId };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetByCustomerId(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Single(orders);
        }

        [Fact]
        public async Task UpdateOrderStatus_OrderNotFound_ReturnsNotFound()
        {
            Arrange
            var orderId = Guid.NewGuid();
            var status = 2;

            Act
            var result = await _controller.UpdateOrderStatus(orderId, status);

            Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);
            Assert.Equal(404, errorResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_OrderFound_ReturnsOk()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var status = 2;

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedOrder = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(status, updatedOrder.Status);
        }
    }
}
