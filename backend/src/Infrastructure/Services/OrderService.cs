using Domain.Models;
using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Infrastructure.Services
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IOrderItemService _orderItemService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly HttpClient _httpClient;

        public OrderService(AppDbContext context, IOrderItemService orderItemService, 
                            ICustomerService customerService, IProductService productService, 
                            ILogger<OrderService> logger, HttpClient httpClient)
            : base(context)
        {
            _orderItemService = orderItemService;
            _customerService = customerService;
            _productService = productService;
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Corrigido o erro de escopo da variável "order"
                    Order order = new Order
                    {
                        CustomerId = customerId,
                        DeliveryAddress = deliveryAddress,
                        Status = OrderStatus.Enviado,
                        OrderDate = DateTime.UtcNow
                    };

                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Pedido criado com sucesso: {order.Id}");

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
                        product.ProductStockQuantity -= item.Quantity; // Atualiza a quantidade disponível
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return order; // Adicionando o retorno do pedido aqui
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await GetById(orderId);
            if (order == null) return null;

            if (orderUpdateDTO.Status != null)
                order.Status = Enum.Parse<OrderStatus>(orderUpdateDTO.Status);

            order.DeliveryAddress = orderUpdateDTO.DeliveryAddress;

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            // Remover "await" do List<OrderItem> pois não é necessário
            var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
            return orderItems.Sum(oi => oi.OrderItemPrice);
        }

        public async Task<bool> ValidateOrder(Guid orderId)
        {
            var order = await GetById(orderId);
            if (order == null) return false;

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
            return product != null && product.ProductStockQuantity >= quantity;
        }

        public async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/cep/{deliveryAddress}");
            return response.IsSuccessStatusCode;
        }
    }
}
