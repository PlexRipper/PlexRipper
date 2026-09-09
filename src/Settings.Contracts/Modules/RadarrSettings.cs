namespace Reaparr.Settings.Contracts;

public record RadarrSettings : BaseSettingsModule<RadarrSettings>, IRadarrSettings
{
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
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    public required string RadarrBaseUrl
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <inheritdoc/>
    public required string RadarrApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsValidApiKey() => !string.IsNullOrWhiteSpace(RadarrApiKey);
}
