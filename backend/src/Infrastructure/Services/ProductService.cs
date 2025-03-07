using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly HttpClient _httpClient; // Adicionando a definição do HttpClient

        public ProductService(AppDbContext context, HttpClient httpClient) : base(context)
        {
            _httpClient = httpClient; // Inicializando o HttpClient
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetByName(string name)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductName.ToLower() == name.ToLower());
        }

        public async Task<bool> CheckStockAvailability(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;
            return product.ProductStockQuantity >= quantity;
        }

        public async Task<Product> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProduct(Product product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                return null; // Produto não encontrado
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.ProductPrice = product.ProductPrice;
            existingProduct.ProductStockQuantity = product.ProductStockQuantity;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task DeleteProduct(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateProductStock(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.ProductStockQuantity >= quantity;
        }

        public async Task<bool> ValidateDeliveryAddress(string deliveryAddress)
        {
            var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/cep/{deliveryAddress}");
            return response.IsSuccessStatusCode;
        }

        // Implementação do método GetPriceById para retornar o preço do produto com base no ID
        public async Task<decimal> GetPriceById(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new Exception("Produto não encontrado.");

            return product.ProductPrice;
        }
    }
}
