using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt; // Adicionando a diretiva de namespace para JwtSecurityTokenHandler
using Microsoft.Extensions.Logging; // Adicionando a diretiva de namespace para ILogger

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
        private readonly ILogger<OrderController> _logger; // Adicionando a dependência do logger

        public OrderController(
            IOrderService orderService, 
            IProductService productService, 
            ICustomerService customerService, 
            AppDbContext context,
            ILogger<OrderController> logger) : base(orderService)
        {
            _orderService = orderService;
            _productService = productService;
            _customerService = customerService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequestDTO orderRequest) 
        {
            if (orderRequest == null || orderRequest.OrderItems == null || !orderRequest.OrderItems.Any())
            {
                _logger.LogWarning("Pedido recebido com dados inválidos ou sem itens.");
                return BadRequest("Pedido não pode ser vazio ou sem itens.");
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var role = jsonToken?.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            _logger.LogInformation("Token extraído, verificando o papel do usuário.");

            if (role != "CLIENTE")
            {
                _logger.LogWarning($"Acesso negado: Usuário com papel '{role}' não autorizado.");
                return BadRequest("Cliente não encontrado ou não autorizado.");
            }

            if (Guid.TryParse(userId, out Guid parsedUserId))
            {
                var customer = await _customerService.GetByUserId(parsedUserId);
                if (customer == null)
                {
                    _logger.LogWarning($"Cliente com UserId {parsedUserId} não encontrado.");
                    return BadRequest("Cliente não encontrado.");
                }

                _logger.LogInformation($"Criando pedido para o Cliente ID: {customer.Id}");

                Order order = new Order
                {
                    CustomerId = customer.Id,
                    DeliveryAddress = orderRequest.DeliveryAddress,
                    DeliveryZipCode = orderRequest.DeliveryZipCode,
                    Status = OrderStatus.Enviado,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = orderRequest.OrderItems.Sum(item => item.Quantity * item.Price)
                };

                // Logando os detalhes do pedido
                _logger.LogInformation($"Total do pedido: {order.TotalAmount} para o Cliente ID: {customer.Id}, Endereço: {order.DeliveryAddress}");

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Processando os itens do pedido
                        foreach (var item in orderRequest.OrderItems)
                        {
                            var product = await _productService.GetById(item.ProductId);
                            _logger.LogInformation($"Processando item de pedido: Produto ID: {item.ProductId}, Quantidade: {item.Quantity}");

                            if (product == null)
                            {
                                _logger.LogWarning($"Produto com ID {item.ProductId} não encontrado.");
                                return BadRequest($"Produto com ID {item.ProductId} não encontrado.");
                            }

                            if (product.ProductStockQuantity < item.Quantity)
                            {
                                _logger.LogWarning($"Quantidade insuficiente para o produto {product.ProductName}. Estoque: {product.ProductStockQuantity}, Pedido: {item.Quantity}");
                                return BadRequest($"Quantidade de {product.ProductName} em estoque insuficiente.");
                            }

                            // Atualizando estoque após a adição ao pedido
                            product.ProductStockQuantity -= item.Quantity;
                            await _productService.UpdateProduct(product);

                            OrderItem orderItem = new OrderItem
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Price = item.Price,
                                Order = order
                            };

                            _logger.LogInformation($"Adicionando item ao pedido: Produto ID: {item.ProductId}, Quantidade: {item.Quantity}, Preço: {item.Price}");

                            order.OrderItems.Add(orderItem);
                        }

                        await _context.Orders.AddAsync(order);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Pedido {order.Id} criado com sucesso!");

                        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Erro ao criar o pedido: {ex.Message}");
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Erro interno ao processar o pedido.");
                    }
                }
            }
            else
            {
                _logger.LogWarning("ID de usuário inválido.");
                return BadRequest("ID de usuário inválido.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                _logger.LogWarning($"Pedido com ID {id} não encontrado.");
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, OrderUpdateDTO orderUpdateDTO)
        {
            if (orderUpdateDTO == null)
            {
                _logger.LogWarning("Dados de atualização do pedido não podem ser nulos.");
                return BadRequest("Order update data cannot be null.");
            }

            var updatedOrder = await _orderService.UpdateOrder(id, orderUpdateDTO);
            if (updatedOrder == null)
            {
                _logger.LogWarning($"Pedido com ID {id} não encontrado para atualização.");
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
                _logger.LogWarning($"Pedido com ID {id} não encontrado para exclusão.");
                return NotFound();
            }

            await _orderService.DeleteOrder(id);
            _logger.LogInformation($"Pedido com ID {id} excluído com sucesso.");
            return NoContent();
        }
    }
}
