namespace Reaparr.Settings.Contracts;

public record RadarrSettings : BaseSettingsModule<RadarrSettings>, IRadarrSettings
{
    private string _radarrBaseUrl = string.Empty;
    private string _radarrApiKey = string.Empty;
    private bool _isConfigured;

    public static RadarrSettings Create() =>
        new()
        {
            IsConfigured = false,
            RadarrBaseUrl = "http://localhost:7878",
            RadarrApiKey = string.Empty,
        };

    /// <inheritdoc/>
    public required bool IsConfigured
    {
        get => _isConfigured;
        set => SetProperty(ref _isConfigured, value);
    }

    /// <inheritdoc/>
    public required string RadarrBaseUrl
    {
        get => _radarrBaseUrl;
        set => SetProperty(ref _radarrBaseUrl, value);
    }

    /// <inheritdoc/>
    public required string RadarrApiKey
    {
        get => _radarrApiKey;
        set => SetProperty(ref _radarrApiKey, value);
    }

    public bool IsValidUrl() =>
        !string.IsNullOrWhiteSpace(RadarrBaseUrl)
        && Uri.TryCreate(RadarrBaseUrl, UriKind.Absolute, out var uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public bool IsValidApiKey() => !string.IsNullOrWhiteSpace(RadarrApiKey);
}
