namespace Reaparr.Settings.Contracts;

public interface ISonarrSettings
{
    /// <summary>
    /// Gets or sets the base URL of the Sonarr instance.
    /// <example>http://localhost:8989</example>
    /// </summary>
    string SonarrBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr API Key used to authenticate API calls to Sonarr.
    /// </summary>
    string SonarrApiKey { get; set; }



    bool IsValidUrl();

    bool IsValidApiKey();
}
