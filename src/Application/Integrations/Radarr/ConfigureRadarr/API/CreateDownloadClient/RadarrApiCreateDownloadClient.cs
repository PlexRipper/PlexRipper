namespace Reaparr.Application;

public record RadarrApiCreateDownloadClientCommand : ICommand<Result<RadarrDownloadClientResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required RadarrDownloadContractDTO Resource { get; init; }
}

public class RadarrApiCreateDownloadClientCommandHandler
    : ICommandHandler<RadarrApiCreateDownloadClientCommand, Result<RadarrDownloadClientResourceDTO>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiCreateDownloadClientCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrDownloadClientResourceDTO>> ExecuteAsync(
        RadarrApiCreateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<RadarrDownloadClientResourceDTO>();

        using var client = clientResult.Value;
        return await client.CreateRadarrDownloadClientAsync(command.Resource, cancellationToken);
    }
}
