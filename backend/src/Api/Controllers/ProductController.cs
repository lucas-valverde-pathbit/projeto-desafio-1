using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Para adicionar o log

namespace Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : BaseController<Product, IProductService>
    {
        private readonly ILogger<ProductController> _logger; // Adicionando o logger

        public ProductController(IProductService service, ILogger<ProductController> logger) : base(service)
        {
            _logger = logger; // Injetando o logger
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Produto não pode ser nulo.");
            }

            try
            {
                // Adicionando um log para saber quando um produto está sendo criado
                _logger.LogInformation("Criando um novo produto com o nome: {ProductName}", product.ProductName);
                
                var createdProduct = await _service.Add(product);

                // Log de todos os produtos após a criação para confirmar a adição
                var allProducts = await _service.GetAll();
                _logger.LogInformation("Total de produtos após criação: {TotalProducts}", allProducts?.Count() ?? 0);

                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                // Logando o erro real
                _logger.LogError("Erro ao criar produto: {ErrorMessage}", ex.Message);

                // Retorna uma resposta com código 500 e a mensagem do erro
                return StatusCode(500, $"Erro ao criar o produto: {ex.Message}");
            }
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
