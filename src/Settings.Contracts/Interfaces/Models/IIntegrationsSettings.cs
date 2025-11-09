namespace Reaparr.Settings.Contracts;

public interface IIntegrationsSettings
{
    /// <summary>
    /// Gets or sets the Reaparr API Key used by Sonarr to send API calls to Reaparr.
    /// </summary>
    string ReaparrApiKey { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr integration settings.
    /// </summary>
    SonarrSettings Sonarr { get; set; }
    
    /// <summary>
    /// Gets or sets the Radarr integration settings.
    /// </summary>
    RadarrSettings Radarr { get; set; }
}