namespace Reaparr.Application;

public record SonarrApiCreateIndexerCommand : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrIndexerContractDTO Resource { get; init; }
}

public class SonarrApiCreateIndexerCommandHandler
    : ICommandHandler<SonarrApiCreateIndexerCommand, Result<SonarrIndexerContractDTO>>
{
    private readonly ILogger _log;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarrApiCreateIndexerCommandHandler(ILogger logger, ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _log = logger.ForContext<SonarrApiCreateIndexerCommandHandler>();
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiCreateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Creating Sonarr indexer with name {IndexerName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Content = json.ToStringContent();

            var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId, cancellationToken);
            if (clientResult.IsFailed)
                return clientResult.ToResult<SonarrIndexerContractDTO>();

            using var client = clientResult.Value;
            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to create indexer in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var created = JsonSerializer.Deserialize<SonarrIndexerContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new SonarrIndexerContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
