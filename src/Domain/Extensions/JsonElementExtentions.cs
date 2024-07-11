using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Domain;

public static class JsonElementExtentions
{
    public static object GetTypedValue(this JsonElement jsonElement, Type type)
    {
        return type switch
        {
            { } t when t == typeof(int) => jsonElement.GetInt32(),
            { } t when t == typeof(bool) => jsonElement.GetBoolean(),
            { } t when t == typeof(string) => jsonElement.GetString(),
            { } t when t == typeof(ViewMode) => jsonElement.GetString().ToViewMode(),
            { } t when t == typeof(List<PlexServerSettingsModel>)
                => JsonSerializer.Deserialize<List<PlexServerSettingsModel>>(
                    jsonElement.GetRawText(),
                    DefaultJsonSerializerOptions.ConfigManagerOptions
                ),
            _ => throw new ArgumentException($"Typename {type.FullName} of {type} is not supported when parsing")
        };
    }
}
