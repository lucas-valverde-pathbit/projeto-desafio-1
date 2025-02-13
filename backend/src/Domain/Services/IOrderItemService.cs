using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Services
{
    public interface IOrderItemService : IBaseService<OrderItem>
    {
        Task<OrderItem> AddOrderItem(Guid orderId, Guid productId, int quantity);
        Task<OrderItem?> UpdateOrderItem(Guid orderItemId, int quantity);
        Task<bool> RemoveOrderItem(Guid orderItemId);
        Task<decimal> CalculateTotalPrice(Guid orderId);
    }
}
