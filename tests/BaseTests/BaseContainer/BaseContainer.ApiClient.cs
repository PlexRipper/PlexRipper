using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlexRipper.BaseTests;

public partial class BaseContainer
{
    public Task<HttpResponseMessage> PostAsync(string requestUri, object body)
    {
        return ApiClient.PostAsync(requestUri,
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") },
            });
    }

    public Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        return ApiClient.GetAsync(requestUri);
    }
}