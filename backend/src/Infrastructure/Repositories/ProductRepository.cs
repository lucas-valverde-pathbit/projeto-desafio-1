using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
public class ProductRepository : BaseRepository<Product>, IRepository<Product>

    {
        public ProductRepository(AppDbContext context) : base(context) { }

        public async Task<Product?> GetByName(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ProductName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> CheckStockAvailability(Guid productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            return product != null && product.ProductStockQuantity >= quantity;
        }
    }
}
