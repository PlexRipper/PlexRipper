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
        try
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("/api/v3/indexer", UriKind.Relative)
            );
            var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
            if (clientResult.IsFailed)
                return clientResult.ToResult<List<RadarrIndexerResourceDTO>>();

            using var client = clientResult.Value;

            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to get indexers from Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var list = JsonSerializer.Deserialize<List<RadarrIndexerResourceDTO>>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );
            return Result.Ok(list ?? []);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
