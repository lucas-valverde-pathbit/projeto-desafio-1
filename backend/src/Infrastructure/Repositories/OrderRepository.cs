using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
public class OrderRepository : IRepository<Order>, IBaseService<Order>

    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implementação do método Add (da interface IBaseService)
        public async Task<Order> Add(Order entity)
        {
            await _context.Orders.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Implementação do método GetById (da interface IBaseService)
        public async Task<Order?> GetById(Guid id)
        {
            return await _context.Orders.FindAsync(id);
        }

        // Implementação do método GetAll (da interface IBaseService)
        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _context.Orders.ToListAsync();
        }

        // Implementação do método Update (da interface IBaseService)
        public async Task<Order?> Update(Guid id, Order entity)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null) return null;

            _context.Entry(existingOrder).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingOrder;
        }

        // Implementação do método Delete (da interface IBaseService)
        public async Task<bool> Delete(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementação do método CreateOrder (da interface IOrderService)
        public async Task<Order> CreateOrder(Guid customerId, List<OrderItemDTO> items, string deliveryAddress)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) throw new Exception("Cliente não encontrado.");

            var order = new Order
            {
                CustomerId = customerId,
                DeliveryAddress = deliveryAddress,
                Status = OrderStatus.Enviado,
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.ProductStockQuantity < item.Quantity)
                {
                    throw new Exception($"Estoque insuficiente para o produto {item.ProductId}.");
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    OrderItemQuantity = item.Quantity,
                    OrderItemPrice = product.ProductPrice * item.Quantity
                };

                await _context.OrderItems.AddAsync(orderItem);
                product.ProductStockQuantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            return order;
        }

        // Implementação do método UpdateOrder (da interface IOrderService)
        public async Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.Status = Enum.Parse<OrderStatus>(orderUpdateDTO.Status);
            order.DeliveryAddress = orderUpdateDTO.DeliveryAddress;

            await _context.SaveChangesAsync();
            return order;
        }

        // Implementação do método CalculateTotalPrice (da interface IOrderService)
        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
            return orderItems.Sum(oi => oi.OrderItemPrice);
        }

        // Implementação do método ValidateOrder (da interface IOrderService)
        public async Task<bool> ValidateOrder(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            if (!order.OrderItems.Any())
                return false;

            var validStatuses = new[] { OrderStatus.Enviado, OrderStatus.Pendente, OrderStatus.Entregue, OrderStatus.Cancelado };
            if (!validStatuses.Contains(order.Status))
                return false;

            return true;
        }

        // Implementação do método ValidateProductStock (da interface IOrderService)
        public async Task<bool> ValidateProductStock(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            return product.ProductStockQuantity >= quantity;
        }

        // Implementação do método ValidateDeliveryAddress (da interface IOrderService)
        public async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            return !string.IsNullOrEmpty(deliveryAddress);
        }
    }
}
