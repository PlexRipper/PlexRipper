namespace Reaparr.Application;

public record RadarrApiUpdateIndexerCommand : ICommand<Result<RadarrIndexerResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required RadarrIndexerContractDTO Resource { get; init; }
}

public class RadarrApiUpdateIndexerCommandValidator : Validator<RadarrApiUpdateIndexerCommand>
{
    public RadarrApiUpdateIndexerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class RadarrApiUpdateIndexerCommandHandler
    : ICommandHandler<RadarrApiUpdateIndexerCommand, Result<RadarrIndexerResourceDTO>>
{
    private readonly ILogger _log;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiUpdateIndexerCommandHandler(ILogger logger, IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _log = logger.ForContext<RadarrApiUpdateIndexerCommandHandler>();
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrIndexerResourceDTO>> ExecuteAsync(
        RadarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Updating Radarr indexer with name {IndexerName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
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
                    .Fail($"Failed to update indexer in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<RadarrIndexerResourceDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new RadarrIndexerResourceDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
