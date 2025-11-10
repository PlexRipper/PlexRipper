namespace Reaparr.Settings.Contracts;

public interface IIntegrationsSettings
{
    /// <summary>
    /// Gets or sets the Reaparr API Key used by Sonarr to send API calls to Reaparr.
    /// </summary>
    string ReaparrApiKey { get; set; }

    /// <summary>
    /// Username for qBittorrent-compatible Public API authentication.
    /// </summary>
    string DownloadClientUsername { get; set; }

    /// <summary>
    /// Password for qBittorrent-compatible Public API authentication.
    /// </summary>
    string DownloadClientPassword { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr integration settings.
    /// </summary>
    SonarrSettings Sonarr { get; set; }

    /// <summary>
    /// Gets or sets the Radarr integration settings.
    /// </summary>
    RadarrSettings Radarr { get; set; }
}
