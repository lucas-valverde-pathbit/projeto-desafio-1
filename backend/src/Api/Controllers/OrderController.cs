using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using System.Text.Json; // Adicionando a diretiva de namespace para ReferenceHandler
using System.IdentityModel.Tokens.Jwt; // Adicionando a diretiva de namespace para JwtSecurityTokenHandler
using System.Linq; // Adicionando a diretiva de namespace para LINQ
using Newtonsoft.Json; // Adicionando a diretiva de namespace para JsonConvert

namespace Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : BaseController<Order, IOrderService>
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context; // Adicionando a dependência do DbContext
        private readonly HttpClient _httpClient; // Adicionando a dependência do HttpClient

        public OrderController(
            IOrderService orderService, 
            IProductService productService, 
            ICustomerService customerService, 
            AppDbContext context, 
            HttpClient httpClient) : base(orderService)
        {
            _orderService = orderService;
            _productService = productService;
            _customerService = customerService;
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequestDTO orderRequest) 
        {
            if (orderRequest == null || orderRequest.OrderItems == null || !orderRequest.OrderItems.Any())
            {
                return BadRequest("Pedido não pode ser vazio ou sem itens.");
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var role = jsonToken?.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value; // Obtendo o UserId

            Console.WriteLine($"User role being checked: {role}"); // Log para verificar a role do usuário

            if (role != "CLIENTE")
            {
                Console.WriteLine("User is not authorized to create orders."); // Log para verificar autorização
                return BadRequest("Cliente não encontrado ou não autorizado.");
            }

            // Verifique se userId é válido e converta para Guid
            if (Guid.TryParse(userId, out Guid parsedUserId))
            {
                // Buscar o Customer pelo UserId
                var customer = await _customerService.GetByUserId(parsedUserId); // Passando um Guid para o método
                if (customer == null)
                {
                    Console.WriteLine("Customer not found."); // Log para verificar se o cliente foi encontrado
                    return BadRequest("Cliente não encontrado. Verifique se o UserId está correto.");
                }

                // Criação do pedido
                Order order = new Order
                {
                    CustomerId = customer.Id, // Usar o CustomerId encontrado
                    DeliveryAddress = orderRequest.DeliveryAddress,
                    DeliveryZipCode = orderRequest.DeliveryZipCode,
                    Status = OrderStatus.Enviado,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = orderRequest.OrderItems.Sum(item => item.Quantity * item.Price)
                };

                // Adicionar os itens ao pedido
                foreach (var item in orderRequest.OrderItems)
                {
                    OrderItem orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Order = order
                    };
                    order.OrderItems.Add(orderItem);

                    // Atualizando o estoque do produto
                    var product = await _productService.GetById(item.ProductId); // Recuperando o produto
                    if (product != null)
                    {
                        product.ProductStockQuantity -= item.Quantity;
                        await _productService.UpdateProduct(product); // Persistindo a alteração de estoque
                    }
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.Orders.AddAsync(order);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            else
            {
                return BadRequest("ID de usuário inválido.");
            }
        }

        // Implementação do método ValidateDeliveryAddress
        private async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/cep/{deliveryAddress}");
            Console.WriteLine($"Response from address validation API: {await response.Content.ReadAsStringAsync()}"); // Log the response

            return response.IsSuccessStatusCode;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, OrderUpdateDTO orderUpdateDTO)
        {
            if (orderUpdateDTO == null)
            {
                return BadRequest("Order update data cannot be null.");
            }

            var updatedOrder = await _orderService.UpdateOrder(id, orderUpdateDTO);
            if (updatedOrder == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            await _orderService.DeleteOrder(id);
            return NoContent();
        }
    }
}
