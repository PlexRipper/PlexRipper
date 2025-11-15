namespace Reaparr.Settings.Contracts;

public interface IRadarrSettings
{
    /// <summary>
    /// Gets or sets the base URL of the Radarr instance.
    /// <example>http://localhost:7878</example>
    /// </summary>
    string RadarrBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the Radarr API Key used to authenticate API calls to Radarr.
    /// </summary>
    string RadarrApiKey { get; set; }

    /// <summary>
    /// Indicates whether Radarr has been configured by Reaparr
    /// </summary>
    bool IsConfigured { get; set; }

    /// <summary>
    /// Returns true if <see cref="RadarrBaseUrl"/> is a valid absolute HTTP/HTTPS URL.
    /// </summary>
    bool IsValidUrl();

    /// <summary>
    /// Returns true if <see cref="RadarrApiKey"/> is non-empty.
    /// </summary>
    bool IsValidApiKey();
}
