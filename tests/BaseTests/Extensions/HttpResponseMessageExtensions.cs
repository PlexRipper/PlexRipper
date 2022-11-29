using System.Text.Json;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> Deserialize<T>(this HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonResponse, DefaultJsonSerializerOptions.ConfigBase);
    }
}