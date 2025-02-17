using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Para adicionar o log

namespace Api.Controllers
{
    [Route("api/products")]
    public class ProductController : BaseController<Product, IProductService>
    {
        public ProductController(IProductService service) : base(service) { }
    }
}