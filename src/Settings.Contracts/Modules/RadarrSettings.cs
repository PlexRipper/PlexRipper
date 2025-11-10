namespace Reaparr.Settings.Contracts;

public record RadarrSettings : BaseSettingsModule<RadarrSettings>, IRadarrSettings
{
    private string _baseUrl = string.Empty;
    private string _apiKey = string.Empty;

    public static RadarrSettings Create() => new() { BaseUrl = string.Empty, ApiKey = string.Empty };

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
}
