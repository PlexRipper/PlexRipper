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
    private readonly ILogger _log;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiCreateIndexerCommandHandler(ILogger logger, IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _log = logger.ForContext<RadarrApiCreateIndexerCommandHandler>();
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrIndexerResourceDTO>> ExecuteAsync(
        RadarrApiCreateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Creating Radarr indexer with name {IndexerName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Content = json.ToStringContent();

            var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId, cancellationToken);
            if (clientResult.IsFailed)
                return clientResult.ToResult<RadarrIndexerResourceDTO>();
            using var client = clientResult.Value;
            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to create indexer in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var created = JsonSerializer.Deserialize<RadarrIndexerResourceDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new RadarrIndexerResourceDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
