using System.Text.Json;

namespace Reaparr.Application;

public record RadarrApiCreateIndexerCommand : ICommand<Result<RadarrIndexerResourceDTO>>
{
    public required bool ForceSave { get; init; }
    public required RadarrIndexerContractDTO Resource { get; init; }
}

public class RadarrApiCreateIndexerCommandHandler
    : ICommandHandler<RadarrApiCreateIndexerCommand, Result<RadarrIndexerResourceDTO>>
{
    private readonly ILogger _log;
    private readonly HttpClient _client;

    public RadarrApiCreateIndexerCommandHandler(ILogger logger, IHttpClientFactory httpClientFactory)
    {
        _log = logger.ForContext<RadarrApiCreateIndexerCommandHandler>();
        _client = httpClientFactory.CreateRadarrHttpClient();
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

            var response = await _client.SendAsync(httpRequest, cancellationToken);
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
