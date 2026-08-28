namespace Reaparr.Application;

public static class SonarrIntegrationDTOMapper
{
    public static SonarrIntegrationDTO ToDTO(this SonarrIntegration source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            Url = source.BaseUrl,
            ApiKey = source.SonarrApiKey,
            Category = source.Category,
            DownloadPath = source.DownloadPath,
            ProvisioningState = source.ProvisioningState,
        };
}
