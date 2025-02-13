using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Services
{
    public interface IProductService : IBaseService<Product>
    {
        Task<Product?> GetByName(string name);
        Task<bool> CheckStockAvailability(Guid productId, int quantoty);
    }
}