using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Domain.Services;
using Api.Controllers;
using System.Threading.Tasks;

namespace UnitTests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly CustomerController _customerController;

        public CustomerControllerTests()
        {
            // Criando o mock para o ICustomerService
            _customerServiceMock = new Mock<ICustomerService>();

            // Criando o controlador
            _customerController = new CustomerController(_customerServiceMock.Object);
        }

        [Fact]
        public async Task GetByEmail_ClienteEncontrado_DeveRetornarOk()
        {
            // Arrange
            var email = "cliente@teste.com";
            var expectedCustomer = new Customer { Id = Guid.NewGuid(), CustomerEmail = email, CustomerName = "Cliente Teste" };

            // Configurando o mock para retornar o cliente quando o email for solicitado
            _customerServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync(expectedCustomer);

            // Act
            var result = await _customerController.GetByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal(email, returnValue.CustomerEmail);
            Assert.Equal("Cliente Teste", returnValue.CustomerName);
        }


        [Fact]
        public async Task GetByEmail_ClienteNaoEncontrado_DeveRetornarNotFound()
        {
         // Arrange
            var email = "cliente@teste.com";
            _customerServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync((Customer)null);

         // Act
            var result = await _customerController.GetByEmail(email);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);
    
            Assert.Equal("Cliente não encontrado", errorResponse.Message);
            Assert.Equal(404, errorResponse.StatusCode);
            Assert.Equal("Verifique o e-mail fornecido e tente novamente.", errorResponse.Details);
        }

    }
}
