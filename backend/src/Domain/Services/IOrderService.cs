using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Domain.DTOs;

namespace Domain.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<Order?> CreateOrder(Guid customerId, List<OrderItemDTO> items, string deliveryAddress);
        Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO);
        Task<decimal> CalculateTotalPrice(Guid orderId);
        Task<bool> ValidateOrder(Guid orderId);
        Task<bool> ValidateProductStock(Guid productId, int quantity);
        Task<bool> ValidateDeliveryAddress(string deliveryAddress);
    }
}
