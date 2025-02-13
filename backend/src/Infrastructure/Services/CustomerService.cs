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
    }
}
