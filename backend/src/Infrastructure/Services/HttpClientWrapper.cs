using System.Net.Http;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }
    }
}
