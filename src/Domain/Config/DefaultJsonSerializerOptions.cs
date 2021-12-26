using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain.Config
{
    public static class DefaultJsonSerializerOptions
    {
        public static JsonSerializerOptions Config => new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
        };
    }
}