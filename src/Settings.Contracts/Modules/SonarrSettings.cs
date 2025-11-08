namespace Reaparr.Settings.Contracts;

public record SonarrSettings : BaseSettingsModule<SonarrSettings>, ISonarrSettings
{
    private string _baseUrl = "http://localhost:8989";
    private string _apiKey = string.Empty;

    public static SonarrSettings Create() => new() { BaseUrl = string.Empty, ApiKey = string.Empty };

    public required string BaseUrl
    {
        get => _baseUrl;
        set => SetProperty(ref _baseUrl, value);
    }

    public required string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public bool IsValidUrl() =>
        !string.IsNullOrWhiteSpace(BaseUrl)
        || Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public bool IsValidApiKey() => !string.IsNullOrWhiteSpace(ApiKey);
}
