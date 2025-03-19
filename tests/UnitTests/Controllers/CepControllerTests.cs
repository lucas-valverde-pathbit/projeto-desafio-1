using System;
using Moq.Protected;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Controllers;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Controllers
{
    public class CepControllerTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly CepController _controller;

        public CepControllerTests()
        {
            // Cria o mock para o HttpMessageHandler
            _mockHandler = new Mock<HttpMessageHandler>();

            // Configura o HttpClient para usar o handler mockado
            _httpClient = new HttpClient(_mockHandler.Object);

            // Cria o controller com o HttpClient mockado
            _controller = new CepController(_httpClient);
        }

        // Testando quando o CEP é vazio
        [Fact]
        public async Task GetAddress_ReturnsBadRequest_WhenCepIsEmpty()
        {
            // Act
            var result = await _controller.GetAddress("");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IActionResult>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("CEP não pode ser vazio.", badRequestResult.Value);
        }

        // Testando quando o CEP é nulo
        [Fact]
        public async Task GetAddress_ReturnsBadRequest_WhenCepIsNull()
        {
            // Act
            var result = await _controller.GetAddress(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IActionResult>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("CEP não pode ser vazio.", badRequestResult.Value);
        }

        // Testando a busca de endereço com sucesso
        [Fact]
        public async Task GetAddress_ReturnsOk_WhenCepIsValid()
        {
            // Arrange
            var cep = "12345678";
            var addressData = new List<dynamic>
            {
                new
                {
                    addressName = "Rua Exemplo",
                    districtName = "Bairro Exemplo",
                    cityName = "Cidade Exemplo",
                    stateCode = "EX",
                    countryName = "Brasil",
                    zipCodeFormatted = "12345-678"
                }
            };
            var jsonResponse = JsonConvert.SerializeObject(addressData);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };

            // Configura o mock para retornar a resposta simulada
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _controller.GetAddress(cep);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IActionResult>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("Rua Exemplo, Bairro Exemplo, Cidade Exemplo - EX, Brasil, CEP: 12345-678", value.address);
        }

        // Testando erro na requisição HTTP (erro da API externa)
        [Fact]
        public async Task GetAddress_ReturnsInternalServerError_WhenApiFails()
        {
            // Arrange
            var cep = "12345678";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Error")
            };

            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _controller.GetAddress(cep);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IActionResult>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<dynamic>(statusCodeResult.Value);
            Assert.Contains("Erro ao buscar CEP", error.error.ToString());
        }

        // Testando exceção inesperada
        [Fact]
        public async Task GetAddress_ReturnsInternalServerError_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var cep = "12345678";
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ThrowsAsync(new Exception("Erro inesperado"));

            // Act
            var result = await _controller.GetAddress(cep);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IActionResult>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<dynamic>(statusCodeResult.Value);
            Assert.Contains("Erro inesperado", error.error.ToString());
        }
    }
}
