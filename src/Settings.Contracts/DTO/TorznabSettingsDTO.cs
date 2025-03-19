namespace Settings.Contracts;

public class TorznabSettingsDTO
{
    public bool Enabled { get; set; } = false;

    public string ApiKey { get; set; } = string.Empty;

    public bool RequireApiKey { get; set; } = true;

    public int MaxResults { get; set; } = 100;

    public string DownloadDirectory { get; set; } = string.Empty;

    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// List of Plex server IDs to include in searches.
    /// If empty, all available servers will be searched.
    /// </summary>
    public List<int> IncludedServerIds { get; set; } = new List<int>();
    
    /// <summary>
    /// Whether to search across all available Plex servers
    /// </summary>
    public bool SearchAllServers { get; set; } = true;
}
