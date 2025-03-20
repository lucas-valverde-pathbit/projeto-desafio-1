using System.Net.Http;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }
}
