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
 [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
    {
        if (Guid.TryParse(id, out Guid parsedId) && parsedId != product.Id)
        {
            return BadRequest("O ID do produto não corresponde ao ID fornecido.");
        }

        // Verifique se o produto existe no banco de dados
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
            // Salve as alterações no banco de dados
            await _productService.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao atualizar o produto: {ex.Message}");
        }

        return Ok(existingProduct);  // Retorne o produto atualizado
    }
}
}
