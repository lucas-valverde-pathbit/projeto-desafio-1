using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;
using System;
using System.Threading.Tasks;
using Domain.DTOs;  // Adicionando a referência para o ErrorResponseDTO

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

        // Endpoint para criar um novo produto
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Produto não pode ser nulo.",
                    StatusCode = 400,
                    Details = "Você deve fornecer os dados completos do produto para criação."
                };

                return BadRequest(errorResponse);
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
                // Criando o DTO de erro
                var errorResponse = new ErrorResponseDTO
                {
                    Message = $"Erro ao criar o produto: {ex.Message}",
                    StatusCode = 500,
                    Details = "Houve um erro inesperado ao tentar criar o produto."
                };

                return StatusCode(500, errorResponse);
            }
        }

        // Endpoint para obter um produto por ID
        [HttpGet("{id}")]
        public override async Task<ActionResult<Product>> GetById(Guid id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Produto não encontrado.",
                    StatusCode = 404,
                    Details = "O produto com o ID fornecido não foi encontrado."
                };

                return NotFound(errorResponse);
            }

            return Ok(product);
        }

        // Endpoint para atualizar um produto existente por ID
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProductById(string id, [FromBody] Product product)
        {
            if (!Guid.TryParse(id, out Guid parsedId))
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "ID fornecido não é válido.",
                    StatusCode = 400,
                    Details = "O ID fornecido não pôde ser convertido para um GUID válido."
                };

                return BadRequest(errorResponse);
            }

            // Verifique se o produto existe
            var existingProduct = await _productService.GetById(parsedId);

            if (existingProduct == null)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Produto não encontrado.",
                    StatusCode = 404,
                    Details = "Não foi possível encontrar um produto com o ID fornecido."
                };

                return NotFound(errorResponse);
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
                var errorResponse = new ErrorResponseDTO
                {
                    Message = $"Erro ao atualizar o produto: {ex.Message}",
                    StatusCode = 500,
                    Details = "Ocorreu um erro inesperado ao tentar atualizar o produto."
                };

                return StatusCode(500, errorResponse);
            }

            return Ok(existingProduct);
        }
    }
}
