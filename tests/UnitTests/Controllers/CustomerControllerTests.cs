using System;
using System.Threading.Tasks;
using Api.Controllers;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _mockService;
        private readonly CustomerController _controller;

        public CustomerControllerTests()
        {
            _mockService = new Mock<ICustomerService>();
            _controller = new CustomerController(_mockService.Object);
        }

        // Testa se o GetByEmail retorna Ok quando o cliente for encontrado
        [Fact]
        public async Task GetByEmail_ReturnsOk_WhenCustomerFound()
        {
            // Arrange
            var email = "test@example.com";
            var customer = new Customer { Id = Guid.NewGuid(), CustomerEmail = email, CustomerName = "John Doe" };
            _mockService.Setup(service => service.GetByEmail(email)).ReturnsAsync(customer);

            // Act
            var result = await _controller.GetByEmail(email);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedCustomer = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal(customer.CustomerEmail, returnedCustomer.CustomerEmail);
            Assert.Equal(customer.CustomerName, returnedCustomer.CustomerName);
        }

        // Testa se o GetByEmail retorna NotFound quando o cliente não for encontrado
        [Fact]
        public async Task GetByEmail_ReturnsNotFound_WhenCustomerNotFound()
        {
            // Arrange
            var email = "notfound@example.com";
            _mockService.Setup(service => service.GetByEmail(email)).ReturnsAsync((Customer)null);

            // Act
            var result = await _controller.GetByEmail(email);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var errorMessage = Assert.IsType<dynamic>(notFoundResult.Value);
            Assert.Equal("Cliente não encontrado", errorMessage.message.ToString());
        }

        // Testa se o GetByEmail lida corretamente com exceções
        [Fact]
        public async Task GetByEmail_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var email = "error@example.com";
            _mockService.Setup(service => service.GetByEmail(email)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetByEmail(email);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<dynamic>(statusCodeResult.Value);
            Assert.Contains("Erro inesperado", error.error.ToString());
        }
    }
}
