namespace Reaparr.Application;

public record SonarrApiUpdateIndexerCommand : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrIndexerContractDTO Resource { get; init; }
}

public class SonarrApiUpdateIndexerCommandValidator : Validator<SonarrApiUpdateIndexerCommand>
{
    public SonarrApiUpdateIndexerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarrApiUpdateIndexerCommandHandler
    : ICommandHandler<SonarrApiUpdateIndexerCommand, Result<SonarrIndexerContractDTO>>
{
    private readonly ILogger _log;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarrApiUpdateIndexerCommandHandler(ILogger logger, ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _log = logger.ForContext<SonarrApiUpdateIndexerCommandHandler>();
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Updating Sonarr indexer with name {IndexerName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
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
                    .Fail($"Failed to update indexer in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<SonarrIndexerContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new SonarrIndexerContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
