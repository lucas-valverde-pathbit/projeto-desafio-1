using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;

namespace Infrastructure.Services
{
    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        public CustomerService(AppDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByEmail(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerEmail == email);
        }

        public async Task<Customer?> GetByUserId(Guid userId) // Implementando o método GetByUserId
        {
            return await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
