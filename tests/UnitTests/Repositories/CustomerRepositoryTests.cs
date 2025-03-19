using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.Repositories
{
    public class CustomerRepositoryTests
    {
        private DbContextOptions<AppDbContext> _options;

        public CustomerRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;
        }

        private AppDbContext GetContext()
        {
            return new AppDbContext(_options);
        }

        private CustomerRepository GetRepository()
        {
            return new CustomerRepository(GetContext());
        }

        [Fact]
        public async Task AddAsync_ShouldAddCustomer()
        {
            var context = GetContext();
            var repository = new CustomerRepository(context);
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "test@example.com"
            };

            var result = await repository.AddAsync(customer);

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.CustomerEmail);
            Assert.Contains(result, context.Customers);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnCustomer()
        {
            var context = GetContext();
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "test@example.com"
            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);
            var result = await repository.GetByUserIdAsync(customer.UserId);

            Assert.NotNull(result);
            Assert.Equal(customer.CustomerEmail, result.CustomerEmail);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCustomer()
        {
            var context = GetContext();
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "old@example.com"
            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            var updatedCustomer = new Customer
            {
                Id = customer.Id,
                UserId = customer.UserId,
                CustomerEmail = "new@example.com"
            };

            var repository = new CustomerRepository(context);
            var result = await repository.UpdateAsync(updatedCustomer);

            Assert.True(result);
            var updated = await context.Customers.FindAsync(customer.Id);
            Assert.Equal("new@example.com", updated.CustomerEmail);
        }

        [Fact]
        public async Task Delete_ShouldDeleteCustomer()
        {
            var context = GetContext();
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "delete@example.com"
            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);
            var result = await repository.Delete(customer.Id);

            Assert.True(result);
            var deleted = await context.Customers.FindAsync(customer.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCustomers()
        {
            var context = GetContext();
            var customer1 = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "customer1@example.com"
            };
            var customer2 = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "customer2@example.com"
            };

            await context.Customers.AddAsync(customer1);
            await context.Customers.AddAsync(customer2);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);
            var result = await repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnCustomer()
        {
            var context = GetContext();
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CustomerEmail = "test@example.com"
            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);
            var result = await repository.GetByEmailAsync("test@example.com");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.CustomerEmail);
        }
    }
}
