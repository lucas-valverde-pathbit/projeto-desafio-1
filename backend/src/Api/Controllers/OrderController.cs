using Microsoft.AspNetCore.Mvc;
using Domain.DTOs;
using Domain.Services;
using Domain.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : BaseController<Order, IOrderService>
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService service, IProductService productService, ILogger<OrderController> logger) : base(service)
        {
            _orderService = service;
            _productService = productService;
            _logger = logger;
        }

        private Guid ExtractCustomerIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var customerId = jwtToken.Claims.First(claim => claim.Type == "customerId").Value;
            return Guid.Parse(customerId);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrderWithValidation([FromBody] Order order, [FromHeader] string token)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogWarning("Dados do pedido não fornecidos.");
                    return BadRequest("Dados do pedido não fornecidos.");
                }

                if (order.OrderItems == null || !order.OrderItems.Any())
                {
                    _logger.LogWarning("Nenhum item de produto encontrado no pedido.");
                    return BadRequest("Nenhum item de produto encontrado no pedido.");
                }

                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _productService.GetById(orderItem.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning($"Produto não encontrado: {orderItem.ProductId}");
                        return BadRequest("Produto não encontrado.");
                    }

                    if (orderItem.OrderItemQuantity <= 0)
                    {
                        _logger.LogWarning("Quantidade do produto inválida.");
                        return BadRequest("Quantidade do produto inválida.");
                    }

                    var productStockIsValid = await _orderService.ValidateProductStock(product.Id, orderItem.OrderItemQuantity);
                    if (!productStockIsValid)
                    {
                        _logger.LogWarning($"Quantidade do produto {product.ProductName} não disponível em estoque.");
                        return BadRequest($"Quantidade do produto {product.ProductName} não disponível em estoque.");
                    }
                }

                if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
                {
                    _logger.LogWarning("Endereço de entrega não pode ser vazio.");
                    return BadRequest("Endereço de entrega não pode ser vazio.");
                }

                var isAddressValid = await _orderService.ValidateDeliveryAddress(order.DeliveryAddress);
                if (!isAddressValid)
                {
                    _logger.LogWarning("Endereço de entrega inválido.");
                    return BadRequest("Endereço de entrega inválido.");
                }

                var customerId = ExtractCustomerIdFromToken(token); // Extrair ID do cliente do token
                var createdOrder = await _orderService.CreateOrder(customerId, order.OrderItems.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.OrderItemQuantity
                }).ToList(), order.DeliveryAddress);

                _logger.LogInformation($"Pedido criado com sucesso: {createdOrder.Id}");
                return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido.");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}
