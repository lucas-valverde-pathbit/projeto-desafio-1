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
public class OrderItemRepository : IRepository<OrderItem>, IBaseService<OrderItem>

    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implementação do método Add (da interface IBaseService)
        public async Task<OrderItem> Add(OrderItem entity)
        {
            await _context.OrderItems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Implementação do método GetById (da interface IBaseService)
        public async Task<OrderItem?> GetById(Guid id)
        {
            return await _context.OrderItems.FindAsync(id);
        }

        // Implementação do método GetAll (da interface IBaseService)
        public async Task<IEnumerable<OrderItem>> GetAll()
        {
            return await _context.OrderItems.ToListAsync();
        }

        // Implementação do método Update (da interface IBaseService)
        public async Task<OrderItem?> Update(Guid id, OrderItem entity)
        {
            var existingOrderItem = await _context.OrderItems.FindAsync(id);
            if (existingOrderItem == null) return null;

            _context.Entry(existingOrderItem).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingOrderItem;
        }

        // Implementação do método Delete (da interface IBaseService)
        public async Task<bool> Delete(Guid id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return false;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementação do método AddOrderItem (da interface IOrderItemService)
        public async Task<OrderItem> AddOrderItem(Guid orderId, Guid productId, int quantity)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Pedido não encontrado.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Produto não encontrado.");

            if (product.ProductStockQuantity < quantity)
                throw new Exception("Estoque insuficiente para o produto.");

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

        // Implementação do método RemoveOrderItem (da interface IOrderItemService)
        public async Task<bool> RemoveOrderItem(Guid orderItemId)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem == null) return false;

            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product == null) return false;

            product.ProductStockQuantity += orderItem.OrderItemQuantity;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementação do método UpdateOrderItem (da interface IOrderItemService)
        public async Task<OrderItem?> UpdateOrderItem(Guid orderItemId, int quantity)
        {
            // Encontra o item do pedido
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem == null)
                return null; // Retorna null caso o item do pedido não seja encontrado

            // Encontra o produto associado ao item do pedido
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product == null)
                throw new Exception("Produto não encontrado.");

            // Calcula a diferença de estoque e a atualiza
            var stockDifference = orderItem.OrderItemQuantity - quantity;
            if (product.ProductStockQuantity + stockDifference < 0)
                throw new Exception("Quantidade insuficiente em estoque.");

            // Atualiza o estoque do produto
            product.ProductStockQuantity += stockDifference;

            // Atualiza as informações do item do pedido
            orderItem.OrderItemQuantity = quantity;
            orderItem.OrderItemPrice = product.ProductPrice * quantity;

            await _context.SaveChangesAsync();
            return orderItem;
        }

        // Implementação do método CalculateTotalPrice (da interface IOrderItemService)
        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            // Busca todos os itens do pedido
            var orderItems = await _context.OrderItems
                .Where(item => item.OrderId == orderId)
                .ToListAsync();

            // Calcula e retorna o preço total
            return orderItems.Sum(item => item.OrderItemPrice);
        }
    }
}
