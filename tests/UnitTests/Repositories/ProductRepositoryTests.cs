using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Repositories
{
    public class ProductRepositoryTests
    {
        private DbContextOptions<AppDbContext> _options;

        public ProductRepositoryTests()
        {
            // Configuração do banco de dados em memória
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestProductDatabase")
                .Options;
        }

        // Método para obter o contexto do banco de dados em memória
        private AppDbContext GetContext()
        {
            return new AppDbContext(_options);
        }

        // Método para obter o repositório
        private ProductRepository GetRepository()
        {
            return new ProductRepository(GetContext());
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct_WhenProductExists()
        {
            // Preparação do cenário
            var context = GetContext();
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductStockQuantity = 10
            };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context);

            // Executando o método
            var result = await repository.GetById(product.Id);

            // Verificação dos resultados
            Assert.NotNull(result);
            Assert.Equal(product.ProductName, result.ProductName);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new ProductRepository(context);

            // Executando o método com um ID inexistente
            var result = await repository.GetById(Guid.NewGuid());

            // Verificação dos resultados
            Assert.Null(result);
        }

        [Fact]
        public async Task CheckStockAvailability_ShouldReturnTrue_WhenStockIsSufficient()
        {
            // Preparação do cenário
            var context = GetContext();
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductStockQuantity = 10
            };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context);

            // Executando o método com quantidade suficiente no estoque
            var result = await repository.CheckStockAvailability(product.Id, 5);

            // Verificação dos resultados
            Assert.True(result);
        }

        [Fact]
        public async Task CheckStockAvailability_ShouldReturnFalse_WhenStockIsInsufficient()
        {
            // Preparação do cenário
            var context = GetContext();
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Test Product",
                ProductStockQuantity = 10
            };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context);

            // Executando o método com quantidade insuficiente no estoque
            var result = await repository.CheckStockAvailability(product.Id, 15);

            // Verificação dos resultados
            Assert.False(result);
        }

        [Fact]
        public async Task CheckStockAvailability_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new ProductRepository(context);

            // Executando o método com um produto inexistente
            var result = await repository.CheckStockAvailability(Guid.NewGuid(), 5);

            // Verificação dos resultados
            Assert.False(result);
        }
    }
}
 