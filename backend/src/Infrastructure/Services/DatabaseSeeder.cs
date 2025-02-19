using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DatabaseSeeder
    {
        public async Task SeedAsync(AppDbContext context)
        {
            // Verifica se o banco existe e cria se necessário
            await context.Database.EnsureCreatedAsync();

            // Seed Users
            if (!await context.Users.AnyAsync())
            {
                var users = GetPreconfiguredUsers();
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            // Seed Customers
            List<Customer> customers = new();
            if (!await context.Customers.AnyAsync())
            {
                customers = GetPreconfiguredCustomers();
                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!await context.Products.AnyAsync())
            {
                var products = GetPreconfiguredProducts();
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // Seed Orders
            if (!await context.Orders.AnyAsync() && customers.Count > 0)
            {
                var orders = GetPreconfiguredOrders(customers[0].Id);
                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
            }
        }

        private static List<Customer> GetPreconfiguredCustomers()
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

        private static List<Product> GetPreconfiguredProducts()
        {
            return new List<Product>
            {
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    ProductName = "Produto 1", 
                    ProductDescription = "Descrição do Produto 1",
                    ProductPrice = 10.0m,
                    ProductStockQuantity = 3
                },
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    ProductName = "Produto 2", 
                    ProductDescription = "Descrição do Produto 2",
                    ProductPrice = 20.0m,
                    ProductStockQuantity = 6
                }
            };
        }

        private static List<Order> GetPreconfiguredOrders(Guid customerId)
        {
            return new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    OrderDate = DateTime.UtcNow,
                    CustomerId = customerId
                }
            };
        }

        private static List<User> GetPreconfiguredUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "UsuarioExemplo",
                    UserEmail = "usuario@exemplo.com",
                    UserPassword = ComputeSha256Hash("senhaSegura123"),
                    Role = UserRole.CLIENTE
                }
            };
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
