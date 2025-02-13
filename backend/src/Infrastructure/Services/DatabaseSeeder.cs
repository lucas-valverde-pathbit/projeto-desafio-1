using System;
using System.Linq;
using System.Threading.Tasks;
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
                    new Product { ProductName = "Product One", ProductPrice = 10.99M },
                    new Product { ProductName = "Product Two", ProductPrice = 20.99M }

                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                _logger.LogInformation("Products seeded successfully. Total products: " + products.Length);

            }
        }
    }

}
