namespace Settings.Contracts;

public interface ITorznabSettings
{
    bool IsEnabled { get; set; }
    
    string ApiKey { get; set; }
    
    int MaxResultsPerRequest { get; set; }
    
    List<int> EnabledServerIds { get; set; }
    
    bool EnableWebhookNotifications { get; set; }
    
    string WebhookUrl { get; set; }
    
    bool LogSearchRequests { get; set; }
    
    bool AutoCreateDownloadTasks { get; set; }
    
    int SearchTimeoutSeconds { get; set; }
}