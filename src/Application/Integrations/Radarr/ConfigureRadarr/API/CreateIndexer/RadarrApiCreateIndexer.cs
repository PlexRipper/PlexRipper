namespace Reaparr.Application;

public record RadarrApiCreateIndexerCommand : ICommand<Result<RadarrIndexerResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required bool ForceSave { get; init; }
    public required RadarrIndexerContractDTO Resource { get; init; }
}

public class RadarrApiCreateIndexerCommandHandler
    : ICommandHandler<RadarrApiCreateIndexerCommand, Result<RadarrIndexerResourceDTO>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiCreateIndexerCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrIndexerResourceDTO>> ExecuteAsync(
        RadarrApiCreateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<RadarrIndexerResourceDTO>();

        using var client = clientResult.Value;
        return await client.CreateRadarrIndexerAsync(command.ForceSave, command.Resource, cancellationToken);
    }
}
