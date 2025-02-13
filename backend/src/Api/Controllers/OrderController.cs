using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController<Order, IOrderService>
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService service) : base(service)
        {
            _orderService = service;
        }

        // Sobrescrevendo o método de criação de pedido para incluir a validação do estoque e endereço
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            // Verificar se o objeto order não é nulo
            if (order == null)
            {
                return BadRequest("Dados do pedido não fornecidos.");
            }

            // Verifique se o pedido tem itens
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                return BadRequest("Nenhum item de produto encontrado no pedido.");
            }

            // Iterar sobre os itens do pedido para validar o estoque de cada produto
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.Product == null)
                {
                    return BadRequest("Produto não encontrado.");
                }

                // Verificar se a quantidade de estoque do produto é válida
                if (orderItem.Product.ProductStockQuantity <= 0)
                {
                    return BadRequest("Quantidade do produto inválida.");
                }

                // Verificar se o estoque do produto está disponível para a quantidade solicitada
                var productStockIsValid = await _orderService.ValidateProductStock(orderItem.Product.Id, orderItem.OrderItemQuantity);
                if (!productStockIsValid)
                {
                    return BadRequest($"Quantidade do produto {orderItem.Product.ProductName} não disponível em estoque.");
                }
            }

            // Verificar se o endereço de entrega é válido
            if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
            {
                return BadRequest("Endereço de entrega não pode ser vazio.");
            }

            var isAddressValid = await _orderService.ValidateDeliveryAddress(order.DeliveryAddress);
            if (!isAddressValid)
            {
                return BadRequest("Endereço de entrega inválido.");
            }

            // Criar o pedido
            var createdOrder = await _orderService.Add(order);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }
    }
}
