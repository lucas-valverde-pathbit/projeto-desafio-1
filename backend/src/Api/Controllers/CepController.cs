using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/cep")]
    public class CepController : ControllerBase  // Agora herda de ControllerBase
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

                var formattedAddress = $"{firstAddress.addressName}, {firstAddress.districtName}, {firstAddress.cityName} - {firstAddress.stateCode}, {firstAddress.countryName}, CEP: {firstAddress.zipCodeFormatted}";

                return Ok(new { address = formattedAddress });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = $"Erro ao buscar CEP: {ex.Message}. Please check the provided CEP." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erro inesperado: {ex.Message}" });
            }
        }
    }
}
