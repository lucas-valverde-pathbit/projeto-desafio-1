using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Services;
using Domain.Repositories;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByUserIdAsync(Guid userId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.Id);
            if (existingCustomer == null) return false;

            _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        // Correção do método AddAsync
        public async Task<Customer> AddAsync(Customer entity)
        {
            // Adiciona o cliente no contexto
            await _context.Customers.AddAsync(entity);
            await _context.SaveChangesAsync(); // Salva as mudanças no banco de dados
            return entity; // Retorna a entidade Customer após ser salva
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerEmail == email);
        }

        public async Task<bool> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
