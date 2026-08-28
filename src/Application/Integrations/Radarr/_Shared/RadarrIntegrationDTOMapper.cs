namespace Reaparr.Application;

public static class RadarrIntegrationDTOMapper
{
    public static RadarrIntegrationDTO ToDTO(this RadarrIntegration source) =>
        new()
        {
            Id = source.Id,
            Name = source.DisplayName,
            Url = source.BaseUrl,
            ApiKey = source.RadarrApiKey,
            Category = source.Category,
            DownloadFolderId = source.DownloadFolderId,
            ProvisioningState = source.ProvisioningState,
        };
}
