namespace Reaparr.Domain;

public class SonarrIntegration : BaseEntityGuid
{
    public required string DisplayName { get; set; }
    public required string BaseUrl { get; set; }

    public required string SonarrApiKey { get; set; }
    public required string ReaparrApiKey { get; set; }
    public required string Category { get; set; }
    public int? DownloadFolderId { get; set; }
    public FolderPath? DownloadFolder { get; set; }
    public int? ExternalDownloadClientId { get; set; }
    public int? ExternalIndexerId { get; set; }
    public required IntegrationProvisioningState ProvisioningState { get; set; }
    public ICollection<DownloadTaskBase> DownloadTasks { get; set; } = [];
}
