namespace Settings.Contracts;

public class ServerSettingsDTO : IServerSettings
{
    public required List<PlexServerSettingItemModule> Data { get; init; }
}
