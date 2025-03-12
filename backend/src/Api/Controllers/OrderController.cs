using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
using Domain.Models;
using Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : BaseController<Order, IOrderService>
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

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

        public override async Task<ActionResult<IEnumerable<Order>>> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems) // Include OrderItems
                .ToListAsync();
                
            return Ok(orders);
        }

        public override async Task<ActionResult<Order>> GetById(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems) // Include OrderItems
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // Explicitly load OrderItems if not already loaded
                await _context.Entry(order)
                    .Collection(o => o.OrderItems)
                    .LoadAsync();
            }

            if (order == null)
            {
                _logger.LogWarning($"Pedido com ID {id} não encontrado.");
                return NotFound();
            }

            _logger.LogInformation($"Pedido encontrado com {order.OrderItems.Count} itens.");
            
            var options = new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = true
            };
            
            _logger.LogDebug($"Detalhes dos itens do pedido: {System.Text.Json.JsonSerializer.Serialize(order.OrderItems, options)}");

            return Ok(order);
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
                    Status = orderRequest.Status,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = orderRequest.TotalAmount 
                };

                _logger.LogInformation($"Total do pedido: {order.TotalAmount} para o Cliente ID: {customer.Id}, Endereço: {order.DeliveryAddress}");

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
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

                            product.ProductStockQuantity -= item.Quantity;
                            await _productService.UpdateProduct(product);

                            OrderItem orderItem = new OrderItem
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                ProductName = product.ProductName,
                                ProductDescription = product.ProductDescription,
                                ProductPrice = product.ProductPrice,
                                Order = order
                            };

                            _logger.LogInformation($"Adicionando item ao pedido: Produto: {orderItem.ProductName}, Quantidade: {item.Quantity}, Preço: {orderItem.ProductPrice}");

                            order.OrderItems.Add(orderItem);
                        }

                        await _context.Orders.AddAsync(order);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Pedido {order.Id} criado com sucesso!");

                        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
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

        // Agora, o método GetByCustomerId está fora do método CreateOrder.
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetByCustomerId(Guid customerId)
        {
            try
            {
                var orders = await _orderService.GetByCustomerId(customerId);

                if (orders == null || !orders.Any())
                {
                    _logger.LogWarning($"Nenhuma ordem encontrada para o Cliente ID {customerId}.");
                    return NotFound("Nenhuma ordem encontrada para este cliente.");
                }

                _logger.LogInformation($"Encontradas {orders.Count} ordens para o Cliente ID {customerId}.");
                return Ok(orders);
            }   
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar ordens para o Cliente ID {customerId}: {ex.Message}");
                return StatusCode(500, "Erro interno ao buscar as ordens.");
            }
        }
    }
}
