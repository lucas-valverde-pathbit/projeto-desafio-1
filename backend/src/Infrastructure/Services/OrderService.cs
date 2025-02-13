using Domain.Models;
using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Services;
using Infrastructure.Data;

namespace Infrastructure.Services
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IOrderItemService _orderItemService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context, IOrderItemService orderItemService, 
            ICustomerService customerService, IProductService productService)
            : base(context)
        {
            _orderItemService = orderItemService;
            _customerService = customerService;
            _productService = productService;
            _context = context;
        }

        public async Task<Order?> CreateOrder(Guid customerId, List<OrderItemDTO> items, string deliveryAddress)
        {
            var customer = await _customerService.GetById(customerId);
            if (customer == null)
                throw new Exception("Cliente não encontrado.");

            var isAddressValid = await ValidateDeliveryAddress(deliveryAddress);
            if (!isAddressValid)
                throw new Exception("Endereço de entrega inválido.");

            foreach (var item in items)
            {
                var productStockIsValid = await ValidateProductStock(item.ProductId, item.Quantity);
                if (!productStockIsValid)
                    throw new Exception($"Estoque insuficiente para o produto {item.ProductId}.");
            }

            var order = new Order
            {
                CustomerId = customerId,
                DeliveryAddress = deliveryAddress,
                Status = OrderStatus.Enviado,
                OrderDate = DateTime.UtcNow
            };

            await Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                await _orderItemService.AddOrderItem(order.Id, item.ProductId, item.Quantity);
            }

            return order;
        }

        public async Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await GetById(orderId);
            if (order == null)
                return null;

            order.Status = Enum.Parse<OrderStatus>(orderUpdateDTO.Status);
            order.DeliveryAddress = orderUpdateDTO.DeliveryAddress;

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            return await _orderItemService.CalculateTotalPrice(orderId);
        }

        public async Task<bool> ValidateOrder(Guid orderId)
        {
            var order = await GetById(orderId);
            if (order == null)
                return false;

            if (!order.OrderItems.Any())
                return false;

            var validStatuses = new[] { OrderStatus.Enviado, OrderStatus.Pendente, OrderStatus.Entregue, OrderStatus.Cancelado };
            if (!validStatuses.Contains(order.Status))
                return false;

            return true;
        }

        public async Task<bool> ValidateProductStock(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            return product.ProductStockQuantity >= quantity;
        }

        public async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            return !string.IsNullOrEmpty(deliveryAddress);
        }
    }
}
