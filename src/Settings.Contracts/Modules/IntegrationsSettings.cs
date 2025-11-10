namespace Reaparr.Settings.Contracts;

public record IntegrationsSettings : BaseSettingsModule<IntegrationsSettings>, IIntegrationsSettings
{
    private string _reaparrApiKey = string.Empty;
    private string _downloadClientUsername = string.Empty;
    private string _downloadClientPassword = string.Empty;
    private SonarrSettings _sonarr = SonarrSettings.Create();
    private RadarrSettings _radarr = RadarrSettings.Create();

    public static IntegrationsSettings Create() =>
        new()
        {
            ReaparrApiKey = Guid.NewGuid().ToString(),
            DownloadClientUsername = "ReaparrQBittorrent",
            DownloadClientPassword = Guid.NewGuid().ToString().Replace("-", string.Empty),
            Sonarr = SonarrSettings.Create(),
            Radarr = RadarrSettings.Create(),
        };

    /// <inheritdoc/>
    public required string ReaparrApiKey
    {
        get => _reaparrApiKey;
        set => SetProperty(ref _reaparrApiKey, value);
    }

    /// <inheritdoc/>
    public required string DownloadClientUsername
    {
        get => _downloadClientUsername;
        set => SetProperty(ref _downloadClientUsername, value);
    }

    /// <inheritdoc/>
    public required string DownloadClientPassword
    {
        get => _downloadClientPassword;
        set => SetProperty(ref _downloadClientPassword, value);
    }

    /// <inheritdoc/>
    public required SonarrSettings Sonarr
    {
        get => _sonarr;
        set => SetProperty(ref _sonarr, value);
    }

    /// <inheritdoc/>
    public required RadarrSettings Radarr
    {
        get => _radarr;
        set => SetProperty(ref _radarr, value);
    }
}
