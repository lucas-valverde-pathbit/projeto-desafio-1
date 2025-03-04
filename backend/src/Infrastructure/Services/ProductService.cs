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

        public async Task<Product> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProduct(Product product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                return null; // Produto não encontrado
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.ProductPrice = product.ProductPrice;
            existingProduct.ProductStockQuantity = product.ProductStockQuantity;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task DeleteProduct(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
