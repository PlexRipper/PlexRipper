using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PlexRipper.WebAPI.Config
{
    public static class JsonSerializerOptionsWebApi
    {
        public static void Config(JsonOptions options)
        {
            var config = SerializerConfig();
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = config.PropertyNameCaseInsensitive;
            options.JsonSerializerOptions.PropertyNamingPolicy = config.PropertyNamingPolicy;
            foreach (var converter in config.Converters)
            {
                options.JsonSerializerOptions.Converters.Add(converter);
            }
        }

        public static JsonSerializerOptions SerializerConfig()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
            };
        }
    }
}