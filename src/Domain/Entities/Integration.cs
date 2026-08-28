namespace Reaparr.Domain;

public class Integration : BaseEntityGuid
{
    public const int NameMaxLength = 100;
    public const int BaseUrlMaxLength = 2048;
    public const int ApiKeyMaxLength = 255;
    public const int CategoryMaxLength = 100;
    public const int DownloadPathMaxLength = 2048;
    public const int LastErrorMaxLength = 1000;

    public required IntegrationType Type { get; init; }

    public required string Name { get; set; }

    public required string BaseUrl { get; set; }

    public required string ArrApiKey { get; set; }

    public required string ReaparrApiKey { get; set; }

    public required string Category { get; set; }

    public string? DownloadPath { get; set; }

    public int? ExternalDownloadClientId { get; set; }

    public int? ExternalIndexerId { get; set; }

    public required IntegrationProvisioningState ProvisioningState { get; set; }

    public string? LastError { get; set; }

    public bool InitialSetupPending { get; set; }

    public ICollection<DownloadTaskBase> DownloadTasks { get; set; } = [];
}
