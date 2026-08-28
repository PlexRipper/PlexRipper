namespace Reaparr.Application;

public static class SonarrIntegrationDTOMapper
{
    public static SonarrIntegrationDTO ToDTO(this SonarrIntegration source) =>
        new()
        {
            Id = source.Id,
            Name = source.DisplayName,
            Url = source.BaseUrl,
            ApiKey = source.SonarrApiKey,
            Category = source.Category,
            DownloadFolderId = source.DownloadFolderId,
            ProvisioningState = source.ProvisioningState,
        };
}
