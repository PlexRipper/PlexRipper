namespace Reaparr.Application;

public record RadarrApiGetIndexersCommand(Guid IntegrationId) : ICommand<Result<List<RadarrIndexerResourceDTO>>>;

public class RadarrApiGetIndexersCommandHandler
    : ICommandHandler<RadarrApiGetIndexersCommand, Result<List<RadarrIndexerResourceDTO>>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiGetIndexersCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<List<RadarrIndexerResourceDTO>>> ExecuteAsync(
        RadarrApiGetIndexersCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<List<RadarrIndexerResourceDTO>>();

        using var client = clientResult.Value;
        return await client.GetRadarrIndexersAsync(cancellationToken);
    }
}
