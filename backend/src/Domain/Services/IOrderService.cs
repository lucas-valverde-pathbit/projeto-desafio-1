using Domain.Models;
using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<Order?> CreateOrder(Guid customerId, Guid productId, int quantity, string deliveryAddress);
        Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO);
        Task<Order?> UpdateOrderStatus(Guid orderId, int status); // Adicionando este método
        Task<decimal> CalculateTotalPrice(Guid orderId);
        Task<bool> ValidateOrder(Guid orderId);
        Task<bool> ValidateProductStock(Guid productId, int quantity);
        Task<bool> ValidateDeliveryAddress(string deliveryAddress);
        Task<bool> DeleteOrder(Guid orderId);
        Task<List<Order>> GetByCustomerId(Guid customerId);
    }
}

