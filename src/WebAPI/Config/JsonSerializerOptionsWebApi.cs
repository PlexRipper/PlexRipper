using Microsoft.AspNetCore.Mvc;
using PlexRipper.Domain.Config;

namespace PlexRipper.WebAPI;

public static class JsonSerializerOptionsWebApi
{
    public static void Config(JsonOptions options)
    {
        var config = DefaultJsonSerializerOptions.ConfigBase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = config.PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.PropertyNamingPolicy = config.PropertyNamingPolicy;
        foreach (var converter in config.Converters)
            options.JsonSerializerOptions.Converters.Add(converter);
    }
}