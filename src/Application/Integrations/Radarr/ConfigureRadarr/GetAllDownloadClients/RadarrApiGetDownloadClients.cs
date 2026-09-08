namespace Reaparr.Application;

public record RadarrApiGetDownloadClientsCommand(Guid IntegrationId)
    : ICommand<Result<List<RadarrDownloadClientResourceDTO>>>;

public class RadarrApiGetDownloadClientsCommandHandler
    : ICommandHandler<RadarrApiGetDownloadClientsCommand, Result<List<RadarrDownloadClientResourceDTO>>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiGetDownloadClientsCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<List<RadarrDownloadClientResourceDTO>>> ExecuteAsync(
        RadarrApiGetDownloadClientsCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<List<RadarrDownloadClientResourceDTO>>();

        using var client = clientResult.Value;
        return await client.GetRadarrDownloadClientsAsync(cancellationToken);
    }
}
