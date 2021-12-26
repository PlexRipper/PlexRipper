using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> Deserialize<T>(this HttpResponseMessage response)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse, DefaultJsonSerializerOptions.Config);
        }
    }
}