using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : BaseController<Product, IProductService>
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) : base(productService)
        {
            _productService = productService;
        }

        // Endpoint para obter um produto por ID
        [HttpGet("{id}")]
        public new async Task<ActionResult<Product>> GetById(Guid id)  // Usando 'new' para indicar que está ocultando o método da classe base
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }
            return Ok(product);
        }
    }
}
