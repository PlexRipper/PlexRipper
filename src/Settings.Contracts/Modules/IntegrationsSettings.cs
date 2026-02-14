namespace Reaparr.Settings.Contracts;

public record IntegrationsSettings
    : BaseSettingsModule<IntegrationsSettings>,
        IBaseSettingsModule<IntegrationsSettings>,
        IIntegrationsSettings
{
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
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <inheritdoc/>
    public required string DownloadClientUsername
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <inheritdoc/>
    public required string DownloadClientPassword
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <inheritdoc/>
    public required SonarrSettings Sonarr
    {
        get;
        set => SetProperty(ref field, value);
    } = SonarrSettings.Create();

    /// <inheritdoc/>
    public required RadarrSettings Radarr
    {
        get;
        set => SetProperty(ref field, value);
    } = RadarrSettings.Create();
}
