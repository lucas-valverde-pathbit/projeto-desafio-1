using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Para adicionar o log
using Domain.Services;
using Domain.Models;
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
                // Criando o DTO de erro com as informações necessárias
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Cliente não encontrado",
                    StatusCode = 404,
                    Details = "Verifique o e-mail fornecido e tente novamente."
                };

                // Retornando o erro usando o DTO
                return NotFound(errorResponse);
            }

            return Ok(customer);
        }
    }
}
