using Microsoft.AspNetCore.Mvc;
using Domain.DTOs; // Added using directive for AddressDto
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json; // Certifique-se de que o Newtonsoft.Json está referenciado

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CepController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public CepController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("{cep}")]
        public async Task<IActionResult> GetAddress(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
            {
                return BadRequest("CEP não pode ser vazio.");
            }

            try
            {
                var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/addresses/{cep}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<List<dynamic>>(content);
                var firstAddress = addressData[0];

                // Formata o endereço como string para armazenar no DeliveryAddress
                var formattedAddress = $"{firstAddress.addressName}, {firstAddress.districtName}, {firstAddress.cityName} - {firstAddress.stateCode}, {firstAddress.countryName}, CEP: {firstAddress.zipCodeFormatted}";


                Console.WriteLine($"Address data retrieved: {formattedAddress}");

                return Ok(new { address = formattedAddress });


            }
            catch (HttpRequestException ex)
            {
                // Log the error message for debugging
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return StatusCode(500, new { error = $"Erro ao buscar CEP: {ex.Message}. Please check the provided CEP." });


            }
            catch (Exception ex)
            {
                // Log the unexpected error message for debugging
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return StatusCode(500, new { error = $"Erro inesperado: {ex.Message}" });

            }
        }
    }
}
