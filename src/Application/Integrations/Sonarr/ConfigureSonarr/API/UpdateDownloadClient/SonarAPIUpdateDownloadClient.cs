namespace Reaparr.Application;

public record SonarApiUpdateDownloadClientCommand : ICommand<Result<SonarrDownloadContractDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrDownloadContractDTO Resource { get; init; }
}

public class SonarApiUpdateDownloadClientCommandValidator : Validator<SonarApiUpdateDownloadClientCommand>
{
    public SonarApiUpdateDownloadClientCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarApiUpdateDownloadClientCommandHandler
    : ICommandHandler<SonarApiUpdateDownloadClientCommand, Result<SonarrDownloadContractDTO>>
{
    private readonly ILogger _log;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarApiUpdateDownloadClientCommandHandler(ILogger logger, ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _log = logger.ForContext<SonarApiUpdateDownloadClientCommandHandler>();
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrDownloadContractDTO>> ExecuteAsync(
        SonarApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/downloadclient/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Updating Sonarr download client with name {DownloadClientName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
            httpRequest.Content = json.ToStringContent();

            var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
            if (clientResult.IsFailed)
                return clientResult.ToResult<SonarrDownloadContractDTO>();

            using var client = clientResult.Value;
            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update download client in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<SonarrDownloadContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new SonarrDownloadContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
