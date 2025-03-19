using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        private DbContextOptions<AppDbContext> _options;

        public UserRepositoryTests()
        {
            // Configuração do banco de dados em memória
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestUserDatabase")
                .Options;
        }

        // Método para obter o contexto do banco de dados em memória
        private AppDbContext GetContext()
        {
            return new AppDbContext(_options);
        }

        // Método para obter o repositório
        private UserRepository GetRepository()
        {
            return new UserRepository(GetContext());
        }

        [Fact]
        public async Task Add_ShouldAddUser()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User"
            };

            // Executando o método
            var result = await repository.Add(user);

            // Verificação dos resultados
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.UserEmail);
            Assert.Equal("Test User", result.UserName);

            // Verificando se o usuário foi realmente adicionado ao banco de dados
            var addedUser = await context.Users.FindAsync(result.Id);
            Assert.NotNull(addedUser);
            Assert.Equal("test@example.com", addedUser.UserEmail);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            // Preparação do cenário
            var context = GetContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Executando o método
            var result = await repository.GetByIdAsync(user.Id);

            // Verificação dos resultados
            Assert.NotNull(result);
            Assert.Equal(user.UserEmail, result.UserEmail);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new UserRepository(context);

            // Executando o método com um ID inexistente
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Verificação dos resultados
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser()
        {
            // Preparação do cenário
            var context = GetContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Executando o método
            var result = await repository.GetByEmailAsync("test@example.com");

            // Verificação dos resultados
            Assert.NotNull(result);
            Assert.Equal("Test User", result.UserName);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new UserRepository(context);

            // Executando o método com um email inexistente
            var result = await repository.GetByEmailAsync("notfound@example.com");

            // Verificação dos resultados
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            // Preparação do cenário
            var context = GetContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var updatedUser = new User
            {
                Id = user.Id,
                UserEmail = "updated@example.com",
                UserName = "Updated User"
            };

            var repository = new UserRepository(context);

            // Executando o método
            var result = await repository.UpdateAsync(updatedUser);

            // Verificação dos resultados
            Assert.True(result);

            // Verificando se o usuário foi realmente atualizado
            var updated = await context.Users.FindAsync(user.Id);
            Assert.NotNull(updated);
            Assert.Equal("updated@example.com", updated.UserEmail);
            Assert.Equal("Updated User", updated.UserName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Preparação do cenário
            var context = GetContext();
            var updatedUser = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "notfound@example.com",
                UserName = "Not Found User"
            };

            var repository = new UserRepository(context);

            // Executando o método com um usuário que não existe
            var result = await repository.UpdateAsync(updatedUser);

            // Verificação dos resultados
            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldDeleteUser()
        {
            // Preparação do cenário
            var context = GetContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "test@example.com",
                UserName = "Test User"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Executando o método
            var result = await repository.Delete(user.Id);

            // Verificação dos resultados
            Assert.True(result);

            // Verificando se o usuário foi realmente deletado
            var deletedUser = await context.Users.FindAsync(user.Id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenUserNotFound()
        {
            // Preparação do cenário
            var context = GetContext();
            var repository = new UserRepository(context);

            // Executando o método com um ID inexistente
            var result = await repository.Delete(Guid.NewGuid());

            // Verificação dos resultados
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Preparação do cenário
            var context = GetContext();
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "user1@example.com",
                UserName = "User One"
            };
            var user2 = new User
            {
                Id = Guid.NewGuid(),
                UserEmail = "user2@example.com",
                UserName = "User Two"
            };
            await context.Users.AddAsync(user1);
            await context.Users.AddAsync(user2);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Executando o método
            var users = await repository.GetAllAsync();

            // Verificação dos resultados
            Assert.NotNull(users);
            Assert.Equal(2, users.Count());
        }
    }
}
