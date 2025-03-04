using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
public class ProductRepository : BaseRepository<Product>, IRepository<Product>

    {
        public ProductRepository(AppDbContext context) : base(context) { }

public async Task<Product?> GetById(Guid productId)
{
    return await _dbSet.FindAsync(productId);
}


public async Task<bool> CheckStockAvailability(Guid productId, int quantity)
{
    var product = await GetById(productId);
    return product != null && product.ProductStockQuantity >= quantity;
}

    }
}
