using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DatabaseSeeder
    {
        public async Task SeedAsync(Data.AppDbContext context)

        {
            // Verifica se a tabela Customers existe
            if (await context.Database.CanConnectAsync() && 
                await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Customers\" LIMIT 1") == 1)
            {
                // Seed Customers
                if (!await context.Customers.AnyAsync())
                {
                    await context.Customers.AddRangeAsync(GetPreconfiguredCustomers());
                    await context.SaveChangesAsync();
                }
            }

            // Verifica se a tabela Products existe
            if (await context.Database.CanConnectAsync() && 
                await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Products\" LIMIT 1") == 1)
            {
                // Seed Products
                if (!await context.Products.AnyAsync())
                {
                    await context.Products.AddRangeAsync(GetPreconfiguredProducts());
                    await context.SaveChangesAsync();
                }
            }

            // Verifica se a tabela Orders existe
            if (await context.Database.CanConnectAsync() && 
                await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Orders\" LIMIT 1") == 1)
            {
                // Seed Orders
                if (!await context.Orders.AnyAsync())
                {
                    await context.Orders.AddRangeAsync(GetPreconfiguredOrders());
                    await context.SaveChangesAsync();
                }
            }

            // Verifica se a tabela Users existe
            if (await context.Database.CanConnectAsync() && 
                await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Users\" LIMIT 1") == 1)
            {
                // Seed Users
                if (!await context.Users.AnyAsync())
                {
                    await context.Users.AddRangeAsync(GetPreconfiguredUsers());
                    await context.SaveChangesAsync();
                }
            }

        }

        private static IEnumerable<Customer> GetPreconfiguredCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Cliente Exemplo",
                    CustomerEmail = "cliente@exemplo.com"
                }

            };

        }

        private static IEnumerable<Product> GetPreconfiguredProducts()
        {
            return new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Produto 1", ProductPrice = 10.0m },
                new Product { Id = Guid.NewGuid(), ProductName = "Produto 2", ProductPrice = 20.0m }

            };
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    OrderDate = DateTime.UtcNow,
                    CustomerId = Guid.NewGuid()
                }

            };

        }

        private static IEnumerable<User> GetPreconfiguredUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "UsuarioExemplo",
                    UserEmail = "usuario@exemplo.com",
                    UserPassword = "senhaSegura123",
                    Role = UserRole.CLIENTE
                }

            };

        }

    }
}
