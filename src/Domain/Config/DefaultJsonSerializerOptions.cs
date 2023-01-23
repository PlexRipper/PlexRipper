using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain.Config;

public static class DefaultJsonSerializerOptions
{
    public static JsonSerializerOptions ConfigBase { get; } = new()
    {
        //PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions ConfigCaptialized { get; } = new()
    {
        //PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions ConfigIndented { get; } = new()
    {
        //PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true,
    };

    public static JsonSerializerOptions ConfigManagerOptions => ConfigIndented;
}