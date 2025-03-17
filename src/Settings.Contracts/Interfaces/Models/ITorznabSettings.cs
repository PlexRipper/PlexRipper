namespace Settings.Contracts;

public interface ITorznabSettings
{
    bool Enabled { get; set; }
    
    string ApiKey { get; set; }
    
    bool RequireApiKey { get; set; }
    
    int MaxResults { get; set; }
    
    string DownloadDirectory { get; set; }
    
    bool AutoStart { get; set; }
    
    /// <summary>
    /// List of Plex server IDs to include in searches. 
    /// If empty, all available servers will be searched.
    /// </summary>
    List<int> IncludedServerIds { get; set; }
    
    /// <summary>
    /// Whether to search across all available Plex servers
    /// </summary>
    bool SearchAllServers { get; set; }
}
