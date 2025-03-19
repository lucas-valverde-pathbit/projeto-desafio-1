using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Data;
using Xunit;

namespace UnitTests.Data
{
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryDbContextOptions()
        {
            // Configura um banco de dados em memória para testes
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())  // Usa um nome único para cada execução de teste
                .Options;
        }

        private AppDbContext CreateContext(DbContextOptions<AppDbContext> options)
        {
            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(configuration) { Options = options };
        }

        [Fact]
        public async Task Can_Add_User_To_DbContext()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            using var context = CreateContext(options);
            
            var user = new User { Id = 1, Name = "John Doe", Email = "johndoe@example.com" };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var addedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
            Assert.NotNull(addedUser);
            Assert.Equal("John Doe", addedUser.Name);
            Assert.Equal("johndoe@example.com", addedUser.Email);
        }

        [Fact]
        public async Task Can_Add_Customer_To_DbContext()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            using var context = CreateContext(options);
            
            var customer = new Customer { Id = 1, Name = "Customer 1", Address = "123 Main St" };

            // Act
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            // Assert
            var addedCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Id == 1);
            Assert.NotNull(addedCustomer);
            Assert.Equal("Customer 1", addedCustomer.Name);
            Assert.Equal("123 Main St", addedCustomer.Address);
        }

        [Fact]
        public async Task Can_Retrieve_Orders_And_Their_OrderItems()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            using var context = CreateContext(options);
            
            var order = new Order { Id = 1, OrderDate = DateTime.Now };
            var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2 };

            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            // Act
            var orderWithItems = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == 1);

            // Assert
            Assert.NotNull(orderWithItems);
            Assert.Single(orderWithItems.OrderItems);
            Assert.Equal(1, orderWithItems.OrderItems.First().ProductId);
            Assert.Equal(2, orderWithItems.OrderItems.First().Quantity);
        }

        [Fact]
        public async Task OnDelete_Cascade_Deletes_OrderItems_When_Order_Is_Deleted()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            using var context = CreateContext(options);

            var order = new Order { Id = 1, OrderDate = DateTime.Now };
            var orderItem = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2 };

            context.Orders.Add(order);
            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            // Act
            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            // Assert
            var deletedOrderItem = await context.OrderItems.FirstOrDefaultAsync(oi => oi.OrderId == 1);
            Assert.Null(deletedOrderItem);  // O item do pedido deve ter sido excluído por causa do DeleteBehavior.Cascade
        }
    }
}
