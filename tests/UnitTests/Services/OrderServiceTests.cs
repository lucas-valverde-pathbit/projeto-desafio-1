using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services;
using Infrastructure.Data;

using Domain.Models;
using Domain.DTOs;
using Domain.Services;

namespace UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderItemService> _mockOrderItemService;
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderItemService = new Mock<IOrderItemService>();
            _mockCustomerService = new Mock<ICustomerService>();
            _mockProductService = new Mock<IProductService>();
            var mockContext = new Mock<AppDbContext>(); // Mock do contexto do banco de dados
            _orderService = new OrderService(mockContext.Object, _mockOrderItemService.Object, _mockCustomerService.Object, _mockProductService.Object);
        }

        [Theory]
        [InlineData("123 Main St", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public async Task CreateOrder_ShouldReturnOrder_WhenValidDataProvided(string deliveryAddress, bool expected)
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var items = new List<OrderItemDTO>();
            _mockCustomerService.Setup(s => s.GetById(customerId)).ReturnsAsync(new Customer());

            // Act
            var result = await _orderService.CreateOrder(customerId, items, deliveryAddress);

            // Assert
            Assert.Equal(expected, result != null);
        }

        [Fact]
        public async Task UpdateOrder_ShouldReturnUpdatedOrder_WhenValidDataProvided()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderUpdateDTO = new OrderUpdateDTO { Status = "Enviado", DeliveryAddress = "456 Another St" };
            var existingOrder = new Order { Id = orderId, Status = OrderStatus.Pendente, DeliveryAddress = "123 Main St" };

            _mockOrderItemService.Setup(s => s.GetById(orderId)).ReturnsAsync(existingOrder);

            // Act
            var result = await _orderService.UpdateOrder(orderId, orderUpdateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderUpdateDTO.Status, result.Status);
            Assert.Equal(orderUpdateDTO.DeliveryAddress, result.DeliveryAddress);
        }

        [Fact]
        public async Task CalculateTotalPrice_ShouldReturnTotal_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemDTO = new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 2 };
            var orderItems = new List<OrderItemDTO> { orderItemDTO };
            var totalPrice = 100m;

            _mockOrderItemService.Setup(s => s.CalculateTotalPrice(orderId)).ReturnsAsync(totalPrice);

            // Act
            var result = await _orderService.CalculateTotalPrice(orderId);

            // Assert
            Assert.Equal(totalPrice, result);
        }

        [Fact]
        public async Task ValidateOrder_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockOrderItemService.Setup(s => s.GetById(orderId)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.ValidateOrder(orderId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateProductStock_ShouldReturnFalse_WhenStockIsInsufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var quantity = 5;

            _mockProductService.Setup(s => s.ValidateProductStock(productId, quantity)).ReturnsAsync(false);

            // Act
            var result = await _orderService.ValidateProductStock(productId, quantity);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateDeliveryAddress_ShouldReturnTrue_WhenAddressIsValid()
        {
            // Arrange
            var deliveryAddress = "123 Main St";

            // Act
            var result = await _orderService.ValidateDeliveryAddress(deliveryAddress);

            // Assert
            Assert.True(result);
        }

    }
}
