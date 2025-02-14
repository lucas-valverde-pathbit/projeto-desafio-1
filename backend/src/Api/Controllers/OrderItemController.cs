using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;

namespace Api.Controllers
{
    [Route("api/orderitems")]
    [ApiController]
    public class OrderItemController : BaseController<OrderItem, IOrderItemService>
    {
        public OrderItemController(IOrderItemService service) : base(service) { }
    }
}
