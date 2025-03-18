using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Repositories
{
    public class OrderRepositoryTests
    {
        private DbContextOptions<AppDbContext> _options;

        public OrderRepositoryTests()
        {
            // Configurando o banco de dados em memória para os testes
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestOrderDatabase")
                .Options;
        }

        // Método para obter o contexto do banco de dados em memória
        private AppDbContext GetContext()
        {
            return new AppDbContext(_options);
        }

        // Método para obter o repositório
        private OrderRepository GetRepository()
        {
            return new OrderRepository(GetContext());
        }

        [Fact]
        public async Task GetOrdersByCustomerId_ShouldReturnOrders()
        {
            // Preparação do cenário
            var context = GetContext();
            var customerId = Guid.NewGuid(); // ID de cliente fictício

            var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customerId, OrderDate = DateTime.Now };
            var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customerId, OrderDate = DateTime.Now.AddDays(1) };

            // Adicionando os pedidos no banco em memória
            await context.Orders.AddAsync(order1);
            await context.Orders.AddAsync(order2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            // Executando o método a ser testado
            var orders = await repository.GetOrdersByCustomerId(customerId);

            // Verificando os resultados
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count);
            Assert.All(orders, o => Assert.Equal(customerId, o.CustomerId));
        }

        [Fact]
        public async Task GetOrdersByCustomerId_ShouldReturnEmptyList_WhenNoOrders()
        {
            // Preparação do cenário
            var context = GetContext();
            var customerId = Guid.NewGuid(); // ID de cliente fictício

            var repository = new OrderRepository(context);

            // Executando o método a ser testado quando não há pedidos
            var orders = await repository.GetOrdersByCustomerId(customerId);

            // Verificando os resultados
            Assert.NotNull(orders);
            Assert.Empty(orders);
        }

        [Fact]
        public async Task GetOrdersByCustomerId_ShouldReturnEmptyList_WhenCustomerDoesNotExist()
        {
            // Preparação do cenário
            var context = GetContext();
            var nonExistingCustomerId = Guid.NewGuid(); // ID de cliente que não existe

            // Adicionando um pedido com um ID de cliente diferente
            var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), OrderDate = DateTime.Now };
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            // Executando o método a ser testado com um ID de cliente que não tem pedidos
            var orders = await repository.GetOrdersByCustomerId(nonExistingCustomerId);

            // Verificando os resultados
            Assert.NotNull(orders);
            Assert.Empty(orders);
        }
    }
}
