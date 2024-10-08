namespace Settings.Contracts;

public record PlexServerSettingItemModule : BaseSettingsModule<PlexServerSettingItemModule>
{
    private string _plexServerName = string.Empty;
    private int _downloadSpeedLimit;
    private bool _hidden;

    public string PlexServerName
    {
        get => _plexServerName;
        set => SetProperty(ref _plexServerName, value);
    }

    public required string MachineIdentifier { get; init; }

    public int DownloadSpeedLimit
    {
        get => _downloadSpeedLimit;
        set => SetProperty(ref _downloadSpeedLimit, value);
    }

    public bool Hidden
    {
        get => _hidden;
        set => SetProperty(ref _hidden, value);
    }
}
