namespace Reaparr.Settings.Contracts;

public record SonarrSettings : BaseSettingsModule<SonarrSettings>, ISonarrSettings
{
    public static SonarrSettings Create() =>
        new()
        {
            IsConfigured = false,
            SonarrBaseUrl = "http://localhost:8989",
            SonarrApiKey = string.Empty,
        };

    /// <inheritdoc/>
    public required bool IsConfigured
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    public required string SonarrBaseUrl
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <inheritdoc/>
    public required string SonarrApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsValidUrl() =>
        !string.IsNullOrWhiteSpace(SonarrBaseUrl)
        && Uri.TryCreate(SonarrBaseUrl, UriKind.Absolute, out var uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public bool IsValidApiKey() => !string.IsNullOrWhiteSpace(SonarrApiKey);
}
