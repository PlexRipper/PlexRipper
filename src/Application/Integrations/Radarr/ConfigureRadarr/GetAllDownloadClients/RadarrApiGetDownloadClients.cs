using System.Text.Json;
using FastEndpoints;

namespace Reaparr.Application;

public record RadarrApiGetDownloadClientsCommand : ICommand<Result<List<DownloadClientResourceDTO>>>;

public class RadarrApiGetDownloadClientsCommandHandler
    : ICommandHandler<RadarrApiGetDownloadClientsCommand, Result<List<DownloadClientResourceDTO>>>
{
    private readonly HttpClient _client;

    public RadarrApiGetDownloadClientsCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public async Task<Result<List<DownloadClientResourceDTO>>> ExecuteAsync(
        RadarrApiGetDownloadClientsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("/api/v3/downloadclient", UriKind.Relative)
            );
            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to get download clients from Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var list = JsonSerializer.Deserialize<List<DownloadClientResourceDTO>>(
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
