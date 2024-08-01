namespace Settings.Contracts;

public record PlexServerSettingsModel : BaseSettingsModel<PlexServerSettingsModel>
{
    private string _plexServerName = string.Empty;
    private int _downloadSpeedLimit = 0;
    private bool _hidden = false;

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
