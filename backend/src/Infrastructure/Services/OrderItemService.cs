using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderItemService : IOrderItemService, IBaseService<OrderItem>
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;

        public OrderItemService(AppDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        // Métodos da interface IBaseService
        public async Task<OrderItem> Add(OrderItem entity)
        {
            await _context.OrderItems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<OrderItem?> GetById(Guid id)
        {
            return await _context.OrderItems.FindAsync(id);
        }

        public async Task<IEnumerable<OrderItem>> GetAll()
        {
            return await _context.OrderItems.ToListAsync();
        }

        public async Task<OrderItem?> Update(Guid id, OrderItem entity)
        {
            var existingEntity = await _context.OrderItems.FindAsync(id);
            if (existingEntity == null) return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _context.OrderItems.FindAsync(id);
            if (entity == null) return false;

            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // Métodos da interface IOrderItemService
        public async Task<OrderItem> AddOrderItem(Guid orderId, Guid productId, int quantity)
        {
            var product = await _productService.GetById(productId);
            if (product == null)
            {
                throw new Exception("Produto não encontrado.");
            }

            if (product.ProductStockQuantity < quantity)
            {
                throw new Exception("Quantidade insuficiente em estoque.");
            }

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                OrderItemQuantity = quantity,
                OrderItemPrice = product.ProductPrice * quantity
            };

            await _context.OrderItems.AddAsync(orderItem);

            product.ProductStockQuantity -= quantity;

            await _context.SaveChangesAsync();

            return orderItem;
        }

        public async Task<OrderItem?> UpdateOrderItem(Guid orderItemId, int quantity)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem == null) return null;

            var product = await _productService.GetById(orderItem.ProductId);
            if (product == null) throw new Exception("Produto não encontrado.");

            var stockDifference = orderItem.OrderItemQuantity - quantity;
            if (product.ProductStockQuantity + stockDifference < 0)
            {
                throw new Exception("Quantidade insuficiente em estoque.");
            }

            product.ProductStockQuantity += stockDifference;

            orderItem.OrderItemQuantity = quantity;
            orderItem.OrderItemPrice = product.ProductPrice * quantity;

            await _context.SaveChangesAsync();

            return orderItem;
        }

        public async Task<bool> RemoveOrderItem(Guid orderItemId)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem == null) return false;

            _context.OrderItems.Remove(orderItem);

            var product = await _productService.GetById(orderItem.ProductId);
            if (product != null)
            {
                product.ProductStockQuantity += orderItem.OrderItemQuantity;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
            return orderItems.Sum(oi => oi.OrderItemPrice);
        }
    }
}
