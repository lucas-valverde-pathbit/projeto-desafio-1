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
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly HttpClient _httpClient;

        public OrderService(AppDbContext context, 
                            ICustomerService customerService, 
                            IProductService productService, 
                            ILogger<OrderService> logger, 
                            HttpClient httpClient)
            : base(context)
        {
            _customerService = customerService;
            _productService = productService;
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        // Implementação de CreateOrder
        public async Task<Order?> CreateOrder(Guid customerId, Guid productId, int quantity, string deliveryAddress)
        {
            var customer = await _customerService.GetById(customerId);
            if (customer == null || customer.User == null || customer.User.Role != UserRole.CLIENTE)
                throw new Exception("Cliente não encontrado ou não autorizado.");

            var isAddressValid = await ValidateDeliveryAddress(deliveryAddress);
            if (!isAddressValid)
                throw new Exception("Endereço de entrega inválido.");

            var isProductStockValid = await ValidateProductStock(productId, quantity);
            if (!isProductStockValid)
                throw new Exception($"Estoque insuficiente para o produto {productId}.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Criando o pedido
                    Order order = new Order
                    {
                        CustomerId = customerId,
                        DeliveryAddress = deliveryAddress,
                        DeliveryZipCode = "", // Se você quiser preencher o CEP, adapte conforme necessário
                        Status = OrderStatus.Enviado,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = quantity * (await _productService.GetPriceById(productId)) // Cálculo do preço total
                    };

                    // Criando o item do pedido
                    var orderItem = new OrderItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = await _productService.GetPriceById(productId),
                        Order = order
                    };

                    order.OrderItems.Add(orderItem);

                    // Atualizando estoque do produto
                    var product = await _context.Products.FindAsync(productId);
                    if (product != null)
                    {
                        product.ProductStockQuantity -= quantity;
                    }

                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return order;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        // Implementação de UpdateOrder
        public new async Task<Order?> UpdateOrder(Guid orderId, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await GetById(orderId);
            if (order == null) return null;

            if (orderUpdateDTO.Status != null)
                order.Status = Enum.Parse<OrderStatus>(orderUpdateDTO.Status);

            order.DeliveryAddress = orderUpdateDTO.DeliveryAddress;

            await _context.SaveChangesAsync();
            return order;
        }

        // Implementação de CalculateTotalPrice
        public async Task<decimal> CalculateTotalPrice(Guid orderId)
        {
            var order = await GetById(orderId);
            if (order == null)
                return 0;

            decimal totalPrice = 0;

            // Somando o preço dos itens do pedido
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    totalPrice += item.Quantity * product.ProductPrice;
                }
            }

            return totalPrice;
        }

        // Implementação de ValidateOrder
        public async Task<bool> ValidateOrder(Guid orderId)
        {
            var order = await GetById(orderId);
            if (order == null) return false;

            var validStatuses = new[] { OrderStatus.Enviado, OrderStatus.Pendente, OrderStatus.Entregue, OrderStatus.Cancelado };

            if (!validStatuses.Contains(order.Status))
                return false;

            return true;
        }

        // Implementação de ValidateProductStock
        public async Task<bool> ValidateProductStock(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.ProductStockQuantity >= quantity;
        }

        // Implementação de ValidateDeliveryAddress
        public async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/cep/{deliveryAddress}");
            return response.IsSuccessStatusCode;
        }

        // Implementação de DeleteOrder
        public async Task<bool> DeleteOrder(Guid orderId)
        {
            var order = await GetById(orderId);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
