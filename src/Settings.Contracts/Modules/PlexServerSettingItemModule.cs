namespace Reaparr.Settings.Contracts;

public record PlexServerSettingItemModule : BaseSettingsModule<PlexServerSettingItemModule>
{
    public static PlexServerSettingItemModule Create(string plexServerName, string machineIdentifier) =>
        new()
        {
            PlexServerName = plexServerName,
            MachineIdentifier = machineIdentifier,
            DownloadSpeedLimit = 0,
            Hidden = false,
            AllowStreamDownloader = false,
        };

    // TODO:Update this name when the Server name is updated
    public required string PlexServerName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public required string MachineIdentifier { get; init; }

    public required int DownloadSpeedLimit
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool Hidden
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool AllowStreamDownloader
    {
        get;
        set => SetProperty(ref field, value);
    }
}
