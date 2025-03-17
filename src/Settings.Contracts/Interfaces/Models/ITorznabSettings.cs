namespace Settings.Contracts;

public interface ITorznabSettings
{
    bool Enabled { get; set; }
    
    string ApiKey { get; set; }
    
    bool RequireApiKey { get; set; }
    
    int MaxResults { get; set; }
    
    string DownloadDirectory { get; set; }
    
    bool AutoStart { get; set; }
}
