namespace Settings.Contracts;

public class TorznabSettingsDTO : ITorznabSettings
{
    public required bool IsEnabled { get; set; }
    
    public required string ApiKey { get; set; }
    
    public required int MaxResultsPerRequest { get; set; }
    
    public required List<int> EnabledServerIds { get; set; }
    
    public required bool EnableWebhookNotifications { get; set; }
    
    public required string WebhookUrl { get; set; }
    
    public required bool LogSearchRequests { get; set; }
    
    public required bool AutoCreateDownloadTasks { get; set; }
    
    public required int SearchTimeoutSeconds { get; set; }
}