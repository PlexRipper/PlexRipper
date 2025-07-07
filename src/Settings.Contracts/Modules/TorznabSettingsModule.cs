namespace Settings.Contracts;

public record TorznabSettingsModule : BaseSettingsModule<TorznabSettingsModule>, ITorznabSettings
{
    private bool _isEnabled = false;
    private string _apiKey = string.Empty;
    private int _maxResultsPerRequest = 100;
    private List<int> _enabledServerIds = new();
    private bool _enableWebhookNotifications = false;
    private string _webhookUrl = string.Empty;
    private bool _logSearchRequests = false;
    private bool _autoCreateDownloadTasks = true;
    private int _searchTimeoutSeconds = 30;

    public static TorznabSettingsModule Create() =>
        new()
        {
            IsEnabled = false,
            ApiKey = Guid.NewGuid().ToString("N"),
            MaxResultsPerRequest = 100,
            EnabledServerIds = new List<int>(),
            EnableWebhookNotifications = false,
            WebhookUrl = string.Empty,
            LogSearchRequests = false,
            AutoCreateDownloadTasks = true,
            SearchTimeoutSeconds = 30,
        };

    public required bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public required string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public required int MaxResultsPerRequest
    {
        get => _maxResultsPerRequest;
        set => SetProperty(ref _maxResultsPerRequest, value);
    }

    public required List<int> EnabledServerIds
    {
        get => _enabledServerIds;
        set => SetProperty(ref _enabledServerIds, value);
    }

    public required bool EnableWebhookNotifications
    {
        get => _enableWebhookNotifications;
        set => SetProperty(ref _enableWebhookNotifications, value);
    }

    public required string WebhookUrl
    {
        get => _webhookUrl;
        set => SetProperty(ref _webhookUrl, value);
    }

    public required bool LogSearchRequests
    {
        get => _logSearchRequests;
        set => SetProperty(ref _logSearchRequests, value);
    }

    public required bool AutoCreateDownloadTasks
    {
        get => _autoCreateDownloadTasks;
        set => SetProperty(ref _autoCreateDownloadTasks, value);
    }

    public required int SearchTimeoutSeconds
    {
        get => _searchTimeoutSeconds;
        set => SetProperty(ref _searchTimeoutSeconds, value);
    }
}