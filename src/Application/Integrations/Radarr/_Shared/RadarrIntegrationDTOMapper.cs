namespace Reaparr.Application;

public static class RadarrIntegrationDTOMapper
{
    public static RadarrIntegrationDTO ToDTO(this RadarrIntegration source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            Url = source.BaseUrl,
            ApiKey = source.RadarrApiKey,
            Category = source.Category,
            DownloadPath = source.DownloadPath,
            ProvisioningState = source.ProvisioningState,
        };
}
