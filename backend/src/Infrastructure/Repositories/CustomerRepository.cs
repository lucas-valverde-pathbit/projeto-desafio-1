using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Services; // Added this line

namespace Infrastructure.Repositories
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> Add(Customer entity)
        {
            await _context.Customers.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Customer?> GetById(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<IEnumerable<Customer>> GetAll()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> Update(Guid id, Customer entity)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null) return null;

            _context.Entry(existingCustomer).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingCustomer;
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
