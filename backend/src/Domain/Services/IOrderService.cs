using Domain.Models;
using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<Order?> CreateOrder(Guid customerId, Guid productId, int quantity, string deliveryAddress);  // Correção aqui
        Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO);
        Task<decimal> CalculateTotalPrice(Guid orderId);
        Task<bool> ValidateOrder(Guid orderId);
        Task<bool> ValidateProductStock(Guid productId, int quantity);
        Task<bool> ValidateDeliveryAddress(string deliveryAddress);
        Task<bool> DeleteOrder(Guid orderId);
    }
}
