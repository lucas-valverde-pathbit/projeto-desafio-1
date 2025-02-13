using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        public ProductService(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetByName(string name)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> CheckStockAvailability(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;
            return product.ProductStockQuantity >= quantity;
        }
    }
}
