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
    }
}