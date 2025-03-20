using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Domain.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/cep")]
    public class CepController : ControllerBase
    {
        private readonly IHttpClientWrapper _httpClientWrapper;

        public CepController(IHttpClientWrapper httpClientWrapper)
        {
            _httpClientWrapper = httpClientWrapper;
        }

        [HttpGet("{cep}")]
        public async Task<IActionResult> GetAddress(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "CEP não pode ser vazio.",
                    StatusCode = 400,
                    Details = "O CEP fornecido não pode ser vazio ou nulo."
                };
                return BadRequest(errorResponse);
            }

            try
            {
                var response = await _httpClientWrapper.GetAsync($"https://ceprapido.com.br/api/addresses/{cep}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<List<dynamic>>(content);
                var firstAddress = addressData[0];

                var formattedAddress = $"{firstAddress.addressName}, {firstAddress.districtName}, {firstAddress.cityName} - {firstAddress.stateCode}, {firstAddress.countryName}, CEP: {firstAddress.zipCodeFormatted}";

                return Ok(new { address = formattedAddress });
            }
            catch (HttpRequestException ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao buscar CEP.",
                    StatusCode = 500,
                    Details = $"Erro de requisição ao buscar o CEP: {ex.Message}. Por favor, verifique o CEP fornecido."
                };
                return StatusCode(500, errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro inesperado.",
                    StatusCode = 500,
                    Details = $"Erro inesperado ao processar a solicitação: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}

