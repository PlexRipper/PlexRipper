namespace Reaparr.Domain;

public class RadarrIntegration : BaseEntityGuid
{
    public required string DisplayName { get; set; }
    public required string BaseUrl { get; set; }
    public required string RadarrApiKey { get; set; }
    public required string QBittorrentApiKey { get; set; }
    public required string TorznabApiKey { get; set; }
    public required string Category { get; set; }
    public int? DownloadFolderId { get; set; }
    public FolderPath? DownloadFolder { get; set; }
    public int? ExternalDownloadClientId { get; set; }
    public int? ExternalIndexerId { get; set; }
    public required IntegrationProvisioningState ProvisioningState { get; set; }
    public TestConnectionStatus LastConnectionTestStatus { get; set; }
    public int? LastConnectionTestHttpStatusCode { get; set; }
    public string? LastConnectionTestErrorMessage { get; set; }
    public DateTime? LastConnectionTestedAt { get; set; }
    public ICollection<DownloadTaskBase> DownloadTasks { get; set; } = [];
}
