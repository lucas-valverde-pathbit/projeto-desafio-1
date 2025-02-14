using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Para adicionar o log
using Domain.Services;
using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : BaseController<Customer, ICustomerService>
    {
        private readonly ILogger<CustomerController> _logger; // Adicionando o logger

        public CustomerController(ICustomerService service, ILogger<CustomerController> logger) : base(service)
        {
            _logger = logger; // Injetando o logger
        }

        // Método para obter todos os clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            var customers = await _service.GetAll();
            return Ok(customers);
        }

        // Método para obter um cliente por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(Guid id)
        {
            var customer = await _service.GetById(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // Método para criar um novo cliente
        [HttpPost]
        public async Task<ActionResult<Customer>> Create([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer cannot be null.");
            }
            try
            {
                var createdCustomer = await _service.Add(customer);
                var allCustomers = await _service.GetAll(); // Log all customers after creation
                Console.WriteLine("All customers after creation: " + (allCustomers != null ? allCustomers.Count() : 0)); // Log all customers after creation
                _logger.LogInformation("Criando um novo cliente com o nome: {CustomerName}", customer.CustomerName); // Log de criação

                return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao criar cliente: {ErrorMessage}", ex.Message); // Logando o erro
                return StatusCode(500, $"Erro ao criar o cliente: {ex.Message}");
            }
        }

        // Método para atualizar um cliente existente
        [HttpPut("{id}")]
        public async Task<ActionResult<Customer>> Update(Guid id, [FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer cannot be null.");
            }

            var updatedCustomer = await _service.Update(id, customer);
            if (updatedCustomer == null)
            {
                return NotFound();
            }

            return Ok(updatedCustomer);
        }

        // Método para excluir um cliente
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleted = await _service.Delete(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        // Método para testar a conexão (opcional)
        [HttpGet("test-connection")]
        public ActionResult<string> TestConnection()
        {
            try
            {
                // Lógica para testar a conexão com o banco de dados (opcional)
                return "Conexão com o banco de dados está funcionando!";
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao conectar ao banco de dados: {ex.Message}");
            }
        }
    }
}
