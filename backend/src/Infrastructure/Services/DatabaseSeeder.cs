using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Adicionando o namespace para ToListAsync

using Domain.Models;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class DatabaseSeeder
    {
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ILogger<DatabaseSeeder> logger)
        {
            _logger = logger;
        }

        public async Task SeedAsync(AppDbContext context)
        {
            if (!context.Customers.Any())
            {
                var customers = new[]
                {
                    new Customer { CustomerEmail = "customer1@example.com", CustomerName = "Customer One" },
                    new Customer { CustomerEmail = "customer2@example.com", CustomerName = "Customer Two" }
                };

                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
                _logger.LogInformation("Customers seeded successfully.");
            }

            if (!context.Products.Any())
            {
                var products = new[]
                {
                    new Product { ProductName = "Product One", ProductPrice = 10.99M, ProductDescription = "Product One description", ProductStockQuantity = 10},
                    new Product { ProductName = "Product Two", ProductPrice = 20.99M, ProductDescription = "Product Two description", ProductStockQuantity = 20}
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                _logger.LogInformation("Products seeded successfully. Total products: " + products.Length);
            }

            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User { UserEmail = "example1@email.com", UserPassword = "password1"},
                    new User { UserEmail = "example2@email.com", UserPassword = "password2"}
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
                _logger.LogInformation("Users seeded successfully.");
            }

            if (!context.Orders.Any())
            {
                var customers = await context.Customers.ToListAsync(); // Obter os clientes existentes
                var users = await context.Users.ToListAsync(); // Obter os usuários existentes
                
                var orders = new[]
                {
                    new Order { CustomerId = customers[0].Id, DeliveryAddress = "Address One", OrderDate = DateTime.UtcNow, Status = OrderStatus.Pendente, UserId = users[0].Id },
                    new Order { CustomerId = customers[1].Id, DeliveryAddress = "Address Two", OrderDate = DateTime.UtcNow, Status = OrderStatus.Pendente, UserId = users[1].Id }
                };

                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
                _logger.LogInformation("Orders seeded successfully.");
            }

        }
    }
}
