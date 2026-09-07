namespace Reaparr.Application;

public record RadarrApiUpdateDownloadClientCommand : ICommand<Result<RadarrDownloadClientResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }
    public required RadarrDownloadContractDTO Resource { get; init; }
}

public class RadarrApiUpdateDownloadClientCommandValidator : Validator<RadarrApiUpdateDownloadClientCommand>
{
    public RadarrApiUpdateDownloadClientCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class RadarrApiUpdateDownloadClientCommandHandler
    : ICommandHandler<RadarrApiUpdateDownloadClientCommand, Result<RadarrDownloadClientResourceDTO>>
{
    private readonly ILogger _log;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiUpdateDownloadClientCommandHandler(ILogger logger, IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _log = logger.ForContext<RadarrApiUpdateDownloadClientCommandHandler>();
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrDownloadClientResourceDTO>> ExecuteAsync(
        RadarrApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestPath = $"api/v3/downloadclient/{command.Id}".SetQueryParam("forceSave", forceSave).ToString();
            var requestUri = new Uri(requestPath, UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            _log.Here().Debug("Updating Radarr download client with name {DownloadClientName}", command.Resource.Name);
            _log.Here().Debug("Request URI: {RequestUri}, Payload: {Payload}", requestUri, json);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
            httpRequest.Content = json.ToStringContent();

            var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
            if (clientResult.IsFailed)
                return clientResult.ToResult<RadarrDownloadClientResourceDTO>();

            using var client = clientResult.Value;

            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update download client in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<RadarrDownloadClientResourceDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new RadarrDownloadClientResourceDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
