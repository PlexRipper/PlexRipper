namespace Reaparr.Settings.Contracts;

public record SonarrSettings : BaseSettingsModule<SonarrSettings>, ISonarrSettings
{
    private string _sonarrBaseUrl = string.Empty;
    private string _sonarrApiKey = string.Empty;

    public static SonarrSettings Create() =>
        new() { SonarrBaseUrl = "http://localhost:8989", SonarrApiKey = string.Empty };

    /// <inheritdoc/>
    public required string SonarrBaseUrl
    {
        get => _sonarrBaseUrl;
        set => SetProperty(ref _sonarrBaseUrl, value);
    }

    /// <inheritdoc/>
    public required string SonarrApiKey
    {
        get => _sonarrApiKey;
        set => SetProperty(ref _sonarrApiKey, value);
    }

    public bool IsValidUrl() =>
		!string.IsNullOrWhiteSpace(SonarrBaseUrl)
		&& Uri.TryCreate(SonarrBaseUrl, UriKind.Absolute, out var uriResult)
		&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public bool IsValidApiKey() => !string.IsNullOrWhiteSpace(SonarrApiKey);
}
