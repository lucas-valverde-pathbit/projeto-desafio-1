using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Services;
using Domain.DTOs;
using Api.Controllers;
using Microsoft.Extensions.Logging;

namespace UnitTests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<ILogger<OrderController>> _loggerMock;
        private readonly OrderController _orderController;

        public OrderControllerTests()
        {
            // Mocks para os serviços
            _orderServiceMock = new Mock<IOrderService>();
            _productServiceMock = new Mock<IProductService>();
            _customerServiceMock = new Mock<ICustomerService>();
            _loggerMock = new Mock<ILogger<OrderController>>();

            // Criando o controlador
            _orderController = new OrderController(
                _orderServiceMock.Object,
                _productServiceMock.Object,
                _customerServiceMock.Object,
                null,  // Não estamos testando o contexto de banco de dados aqui
                _loggerMock.Object
            );
        }

        #region Testes para CreateOrder

        // Teste 1: Criar Pedido com Sucesso
        [Fact]
        public async Task CreateOrder_DeveRetornarCreated()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var orderRequest = new OrderRequestDTO
            {
                DeliveryAddress = "Endereço de Entrega",
                DeliveryZipCode = "12345-678",
                Status = OrderStatus.Pendente,
                TotalAmount = 100.0M,
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = productId, Quantity = 1 }
                }
            };

            _customerServiceMock.Setup(s => s.GetByUserId(It.IsAny<Guid>())).ReturnsAsync(new Customer());
            _productServiceMock.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(new Product());
            _orderServiceMock.Setup(s => s.CreateOrder(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pendente });

            // Act
            var result = await _orderController.CreateOrder(orderRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
        }

        #endregion

        #region Testes para GetById

        // Teste 2: Obter Pedido por ID com Sucesso
        [Fact]
        public async Task GetById_DeveRetornarOk()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetById(orderId)).ReturnsAsync(new Order { Id = orderId });

            // Act
            var result = await _orderController.GetById(orderId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Order>>(result);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnValue = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(orderId, returnValue.Id);
        }

        // Teste 3: Obter Pedido por ID quando não Encontrado
        [Fact]
        public async Task GetById_DeveRetornarNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetById(orderId)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderController.GetById(orderId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Order>>(result);
            var notFoundResult = actionResult.Result as NotFoundResult;
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region Testes para GetByCustomerId

        // Teste 4: Obter Ordens por CustomerId
        [Fact]
        public async Task GetByCustomerId_DeveRetornarOk()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetByCustomerId(customerId)).ReturnsAsync(new List<Order> { new Order { Id = Guid.NewGuid() } });

            // Act
            var result = await _orderController.GetByCustomerId(customerId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Order>>>(result);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnValue = Assert.IsType<List<Order>>(okResult.Value);
            Assert.NotEmpty(returnValue);
        }

        #endregion

        #region Testes para UpdateOrderStatus

        // Teste 5: Atualizar Status do Pedido com Sucesso
        [Fact]
        public async Task UpdateOrderStatus_DeveRetornarOk()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var status = (int)OrderStatus.Pendente;
            _orderServiceMock.Setup(s => s.UpdateOrderStatus(orderId, status)).ReturnsAsync(new Order { Id = orderId, Status = OrderStatus.Pendente });

            // Act
            var result = await _orderController.UpdateOrderStatus(orderId, status);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Order>>(result);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnValue = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(OrderStatus.Pendente, returnValue.Status);
        }

        // Teste 6: Atualizar Status do Pedido quando Pedido não Existir
        [Fact]
        public async Task UpdateOrderStatus_DeveRetornarNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.UpdateOrderStatus(orderId, 2)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderController.UpdateOrderStatus(orderId, 2);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Order>>(result);
            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal("Pedido não encontrado.", notFoundResult.Value);
        }

        #endregion
    }
}
