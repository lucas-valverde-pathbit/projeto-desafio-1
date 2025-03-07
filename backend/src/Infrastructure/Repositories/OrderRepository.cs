using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IRepository<Order>
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<List<Order>> GetOrdersByCustomerId(Guid customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
