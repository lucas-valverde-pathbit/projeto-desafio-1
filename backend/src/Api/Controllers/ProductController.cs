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
        public ProductController(IProductService service) : base(service) { }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Produto não pode ser nulo.");
            }
            var createdProduct = await _service.Add(product);
            var allProducts = await _service.GetAll(); // Log all products after creation
            Console.WriteLine("All products after creation: " + (allProducts != null ? allProducts.Count() : 0)); // Log all products after creation

            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpGet("test-connection")]
        public ActionResult<string> TestConnection()
        {
            try
            {
                // Aqui você pode adicionar lógica para verificar a conexão com o banco de dados
                return "Conexão com o banco de dados está funcionando!";
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao conectar ao banco de dados: {ex.Message}");
            }
        }
    }
}
