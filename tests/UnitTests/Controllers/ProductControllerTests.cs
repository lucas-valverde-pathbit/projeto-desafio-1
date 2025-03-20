using Xunit;
using Moq;
using Api.Controllers;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Domain.DTOs; // Adicionando a referência para o ErrorResponseDTO

namespace UnitTests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _productController = new ProductController(_productServiceMock.Object);
        }

        // Teste 1: Criar um produto com sucesso
        [Fact]
        public async Task CriacaoDeProdutoTemQueRetornarCreatedQuandoProdutoCriadoEhValido()
        {
            // Arrange
            var product = new Product
            {
                ProductName = "Produto A",
                ProductDescription = "Descrição do Produto A",
                ProductPrice = 10.0M,
                ProductStockQuantity = 100
            };

            var createdProduct = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto A",
                ProductDescription = "Descrição do Produto A",
                ProductPrice = 10.0M,
                ProductStockQuantity = 100
            };

            // Simula o serviço criando o produto
            _productServiceMock.Setup(service => service.CreateProduct(It.IsAny<Product>()))
                               .ReturnsAsync(createdProduct);

            // Act
            var result = await _productController.CreateProduct(product);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(createdProduct.Id, returnValue.Id);
            Assert.Equal("Produto A", returnValue.ProductName);
        }

        // Teste 2: Criar um produto com dados nulos
        [Fact]
        public async Task CriacaoDeProdutoTemQueRetornarBadRequestQuandoProdutoEhNulo()
        {
            // Act
            var result = await _productController.CreateProduct(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(badRequestResult.Value);
            Assert.Equal(400, errorResponse.StatusCode);
            Assert.Equal("Produto não pode ser nulo.", errorResponse.Message);
        }

        // Teste 3: Obter um produto por ID que existe
        [Fact]
        public async Task GetByIdTemQueRetornarProdutoQuandoEleExiste()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, ProductName = "Test Product", ProductDescription = "Description", ProductPrice = 100, ProductStockQuantity = 50 };
            _productServiceMock.Setup(service => service.GetById(productId)).ReturnsAsync(product); // Produto encontrado

            // Act
            var result = await _productController.GetById(productId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);  // Verifica se é ActionResult<Product>
            var okResult = actionResult.Result as OkObjectResult;  // Verifica se é um OkObjectResult
            Assert.NotNull(okResult);  // Verifica se o resultado é Ok
            var returnValue = Assert.IsType<Product>(okResult.Value);  // Verifica se o valor retornado é um produto
            Assert.Equal(product.ProductName, returnValue.ProductName); // Verifica se o nome do produto está correto
            Assert.Equal(product.ProductPrice, returnValue.ProductPrice); // Verifica se o preço do produto está correto
        }

        // Teste 4: Obter um produto por ID que não existe
        [Fact]
        public async Task GetByIdTemQueRetornarNotFoundQuandoProdutoNaoEhEncontrado()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _productServiceMock.Setup(service => service.GetById(productId)).ReturnsAsync((Product)null); // Produto não encontrado

            // Act
            var result = await _productController.GetById(productId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);  // Verifica se é ActionResult<Product>
            var notFoundResult = actionResult.Result as NotFoundObjectResult;  // Verifica se é um NotFoundObjectResult
            Assert.NotNull(notFoundResult);  // Verifica se o resultado é NotFound
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);  // Verifica se é um ErrorResponseDTO
            Assert.Equal(404, errorResponse.StatusCode);  // Verifica o StatusCode
            Assert.Equal("Produto não encontrado.", errorResponse.Message);  // Verifica a mensagem de erro
        }

        // Teste 5: Atualizar um produto com sucesso
        [Fact]
        public async Task UpdateProductByIdTemQueRetornarOkQuandoProdutoEhAtualizado()
        {
            // Arrange
            var productId = Guid.NewGuid().ToString();
            var updatedProduct = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto Atualizado",
                ProductDescription = "Descrição Atualizada",
                ProductPrice = 20.0M,
                ProductStockQuantity = 50
            };

            var existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto Antigo",
                ProductDescription = "Descrição Antiga",
                ProductPrice = 10.0M,
                ProductStockQuantity = 100
            };

            _productServiceMock.Setup(service => service.GetById(It.IsAny<Guid>()))
                               .ReturnsAsync(existingProduct);
            _productServiceMock.Setup(service => service.SaveChangesAsync())
                               .Returns(Task.CompletedTask);

            // Act
            var result = await _productController.UpdateProductById(productId, updatedProduct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(updatedProduct.ProductName, returnValue.ProductName);
            Assert.Equal(updatedProduct.ProductPrice, returnValue.ProductPrice);
        }

        // Teste 6: Atualizar um produto com ID inválido
        [Fact]
        public async Task UpdateProductByIdDeveRetornarBadRequestQuandoIdEhInvalido()
        {
            // Act
            var result = await _productController.UpdateProductById("invalid-id", new Product());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(badRequestResult.Value);
            Assert.Equal(400, errorResponse.StatusCode);
            Assert.Equal("ID fornecido não é válido.", errorResponse.Message);
        }

        // Teste 7: Atualizar um produto que não existe
        [Fact]
        public async Task UpdateProductByIdTemQueRetornarNotFoundQuandoProdutoNaoExiste()
        {
            // Arrange
            var productId = Guid.NewGuid().ToString();
            var updatedProduct = new Product
            {
                ProductName = "Produto Atualizado",
                ProductDescription = "Descrição Atualizada",
                ProductPrice = 20.0M,
                ProductStockQuantity = 50
            };

            _productServiceMock.Setup(service => service.GetById(It.IsAny<Guid>()))
                               .ReturnsAsync((Product)null);

            // Act
            var result = await _productController.UpdateProductById(productId, updatedProduct);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(notFoundResult.Value);
            Assert.Equal(404, errorResponse.StatusCode);
            Assert.Equal("Produto não encontrado.", errorResponse.Message);
        }
    }
}
