namespace Reaparr.Settings.Contracts;

public record PlexServerSettingItemModule : BaseSettingsModule<PlexServerSettingItemModule>
{
    private string _plexServerName = string.Empty;
    private int _downloadSpeedLimit;
    private bool _hidden;

    public static PlexServerSettingItemModule Create(string plexServerName, string machineIdentifier) =>
        new()
        {
            PlexServerName = plexServerName,
            MachineIdentifier = machineIdentifier,
            DownloadSpeedLimit = 0,
            Hidden = false,
        };

    // TODO:Update this name when the Server name is updated
    public required string PlexServerName
    {
        get => _plexServerName;
        set => SetProperty(ref _plexServerName, value);
    }

    public required string MachineIdentifier { get; init; }

    public required int DownloadSpeedLimit
    {
        get => _downloadSpeedLimit;
        set => SetProperty(ref _downloadSpeedLimit, value);
    }

    public required bool Hidden
    {
        get => _hidden;
        set => SetProperty(ref _hidden, value);
    }
}
