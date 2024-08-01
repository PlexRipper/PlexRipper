using System.Text.Json;
using PlexRipper.Domain.Config;

namespace PlexRipper.Settings;

public static class UserSettingsSerializer
{
    public static string Serialize(SettingsModule settingsModule) =>
        JsonSerializer.Serialize(settingsModule, DefaultJsonSerializerOptions.ConfigManagerOptions);

    public static SettingsModel? Deserialize(string json) =>
        JsonSerializer.Deserialize<SettingsModel>(json, DefaultJsonSerializerOptions.ConfigManagerOptions) ?? default;
}
