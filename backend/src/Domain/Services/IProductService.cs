using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IProductService : IBaseService<Product>
    {
        Task<IEnumerable<Product>> GetAll();
        Task<Product?> GetByName(string name);
        Task<bool> CheckStockAvailability(Guid productId, int quantity);
        Task<Product> CreateProduct(Product product);
        Task<Product?> UpdateProduct(Product product);
        Task DeleteProduct(Guid productId);
        Task<bool> ValidateProductStock(Guid productId, int quantity);
        Task<bool> ValidateDeliveryAddress(string deliveryAddress);
        Task<decimal> GetPriceById(Guid productId);
        Task SaveChangesAsync(); // Método adicionado corretamente
    }
}
