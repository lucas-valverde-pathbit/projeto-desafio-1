using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using System.Security.Cryptography;  // Para SHA256
using System.Text;  // Para Encoding e StringBuilder


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

        // Seed Customers (somente se houver clientes com a role CLIENTE)
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>();
            var users = await context.Users.ToListAsync();

            // Para cada usuário, se for CLIENTE, cria um cliente correspondente na tabela Customer
            foreach (var user in users)
            {
                if (user.Role == UserRole.CLIENTE)
                {
                    var customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerName = user.UserName, // Atribuindo o nome do cliente ao nome do usuário
                        CustomerEmail = user.UserEmail, // Atribuindo o email do cliente ao email do usuário
                        UserId = user.Id // Associando o usuário ao cliente
                    };
                    customers.Add(customer);
                }
            }

            // Se houver clientes, insere na tabela de Customers
            if (customers.Count > 0)
            {
                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
            }
        }

        // Seed Products
        if (!await context.Products.AnyAsync())
        {
            var products = GetPreconfiguredProducts();
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // Seed Orders (somente se houver clientes)
        if (!await context.Orders.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var customers = await context.Customers.ToListAsync();
            
            if (users.Any() && customers.Any())
            {
                // Garantindo que a ordem seja associada ao User e ao Customer correto
                var orders = new List<Order>();

                foreach (var customer in customers)
                {
                    var user = users.FirstOrDefault(u => u.Id == customer.UserId); // Encontrar o User que corresponde ao Customer
                    if (user != null && user.Role == UserRole.CLIENTE)
                    {
                        orders.Add(new Order
                        {
                            Id = Guid.NewGuid(),
                            OrderDate = DateTime.UtcNow,
                            CustomerId = customer.Id, // Associando a ordem ao cliente
                            // UserId = user.Id // Associando a ordem ao usuário

                        });
                    }
                }

                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
            }
        }
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
            },
            new Product 
            { 
                Id = Guid.NewGuid(), 
                ProductName = "Produto 3", 
                ProductDescription = "Descrição do Produto 3",
                ProductPrice = 30.0m,
                ProductStockQuantity = 3
            },
            new Product 
            { 
                Id = Guid.NewGuid(), 
                ProductName = "Produto 4", 
                ProductDescription = "Descrição do Produto 4",
                ProductPrice = 40.0m,
                ProductStockQuantity = 4
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
                UserName = "lucasadmin",
                UserEmail = "lucasadmin@gmail.com",
                UserPassword = ComputeSha256Hash("lucasadmin"),
                Role = UserRole.ADMINISTRADOR
            },
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "lucascliente",
                UserEmail = "lucascliente@gmail.com",
                UserPassword = ComputeSha256Hash("lucascliente"),
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
