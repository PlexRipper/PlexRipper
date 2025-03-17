namespace Settings.Contracts;

public class TorznabSettingsDTO
{
    public bool Enabled { get; set; } = false;
    
    public string ApiKey { get; set; } = string.Empty;
    
    public bool RequireApiKey { get; set; } = true;
    
    public int MaxResults { get; set; } = 100;
    
    public string DownloadDirectory { get; set; } = string.Empty;
    
    public bool AutoStart { get; set; } = true;
}
