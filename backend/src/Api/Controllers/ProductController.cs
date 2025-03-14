using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System;
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
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Produto não pode ser nulo.");
            }

            try
            {
                // Chama o método CreateProduct do serviço para salvar o produto
                var createdProduct = await _productService.CreateProduct(product);

                // Retorna o produto criado com status 201 (Created)
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                // Retorna erro 500 se houver alguma falha
                return StatusCode(500, $"Erro ao criar o produto: {ex.Message}");
            }
        }
        // Endpoint para obter um produto por ID
        [HttpGet("{id}")]
        public override async Task<ActionResult<Product>> GetById(Guid id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }
            return Ok(product);
        }
   [HttpPut("update/{id}")]
public async Task<IActionResult> UpdateProductById(string id, [FromBody] Product product)
{
    // Log para verificar os dados recebidos
    Console.WriteLine($"ID recebido: {id}");
    Console.WriteLine($"Dados do produto recebido: {product.ProductName}, {product.ProductDescription}, {product.ProductPrice}, {product.ProductStockQuantity}");

    if (!Guid.TryParse(id, out Guid parsedId))
    {
        return BadRequest("ID fornecido não é válido.");
    }

    // Verifique se o produto existe
    var existingProduct = await _productService.GetById(parsedId);

    if (existingProduct == null)
    {
        return NotFound("Produto não encontrado.");
    }

    // Atualize os campos do produto existente
    existingProduct.ProductName = product.ProductName;
    existingProduct.ProductDescription = product.ProductDescription;
    existingProduct.ProductPrice = product.ProductPrice;
    existingProduct.ProductStockQuantity = product.ProductStockQuantity;

    try
    {
        await _productService.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Erro ao atualizar o produto: {ex.Message}");
    }

    return Ok(existingProduct);
}
}
}
