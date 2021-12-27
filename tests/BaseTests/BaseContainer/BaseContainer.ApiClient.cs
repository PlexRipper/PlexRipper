using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlexRipper.BaseTests
{
    public partial class BaseContainer
    {
        public Task<HttpResponseMessage> PostAsync(string? requestUri, object body)
        {
            return ApiClient.PostAsync(requestUri,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") },
                });
        }
    }
}