using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _productService.GetAll();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(Guid id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Dados do produto não fornecidos.");
            }

            product.Id = Guid.NewGuid();
            var createdProduct = await _productService.CreateProduct(product);
            return CreatedAtAction(nameof(GetAll), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(Guid id, [FromBody] Product product)
        {
            if (product == null || id != product.Id)
            {
                return BadRequest("Dados do produto inválidos.");
            }

            var updatedProduct = await _productService.UpdateProduct(product);
            if (updatedProduct == null)
            {
                return NotFound("Produto não encontrado.");
            }

            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }

            await _productService.DeleteProduct(id);
            return NoContent(); // Retorna 204 No Content
        }
    }
}
