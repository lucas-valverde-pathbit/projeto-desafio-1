using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : BaseController<OrderItem, IOrderItemService>
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService service) : base(service)
        {
            _orderItemService = service;
        }

         [HttpPost("{orderId}/items")]
         public IActionResult AddItemToOrder(Guid orderId, [FromBody] OrderItem orderItem)
        {
            if (orderItem == null || orderItem.ProductId == Guid.Empty || orderItem.OrderItemQuantity <= 0)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                // Criar o item de pedido
                orderItem.OrderId = orderId; // Associando o item ao pedido
                
                // Chama o serviço que manipula a lógica de adicionar o item
                _orderItemService.AddOrderItem(orderItem.ProductId, orderId, orderItem.OrderItemQuantity); 

                return Ok(orderItem); // Retorna o item de pedido adicionado
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao adicionar item ao pedido: {ex.Message}");
            }
        }
    }
}
