using System.Text.Json;
using FastEndpoints;

namespace Reaparr.Application;

public record RadarrApiGetIndexersCommand : ICommand<Result<List<RadarrIndexerContractDTO>>>;

public class RadarrApiGetIndexersCommandHandler
    : ICommandHandler<RadarrApiGetIndexersCommand, Result<List<RadarrIndexerContractDTO>>>
{
    private readonly HttpClient _client;

    public RadarrApiGetIndexersCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public async Task<Result<List<RadarrIndexerContractDTO>>> ExecuteAsync(
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
            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to get indexers from Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var list = JsonSerializer.Deserialize<List<RadarrIndexerContractDTO>>(
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
