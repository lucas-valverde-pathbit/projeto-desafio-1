using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Para adicionar o log
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/customers")]
    public class CustomerController : BaseController<Customer, ICustomerService>
    {
        public CustomerController(ICustomerService service) : base(service) { }
    
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var customer = await _service.GetByEmail(email);
            if (customer == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            return Ok(customer);
        }
   }
}