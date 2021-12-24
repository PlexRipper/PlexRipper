using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        private static readonly JsonSerializerOptions _config = JsonSerializerOptionsWebApi.SerializerConfig();

        public static async Task<T> Deserialize<T>(this HttpResponseMessage response)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse, _config);
        }
    }
}