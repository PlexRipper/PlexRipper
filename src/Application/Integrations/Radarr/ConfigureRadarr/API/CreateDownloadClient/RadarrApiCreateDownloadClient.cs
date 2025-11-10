using System.Text.Json;
using FastEndpoints;

namespace Reaparr.Application;

public record RadarrApiCreateDownloadClientCommand : ICommand<Result<RadarrDownloadClientResourceDTO>>
{
    public required bool ForceSave { get; init; }
    public required RadarrDownloadContractDTO Resource { get; init; }
}

public class RadarrApiCreateDownloadClientCommandHandler
    : ICommandHandler<RadarrApiCreateDownloadClientCommand, Result<RadarrDownloadClientResourceDTO>>
{
    private readonly HttpClient _client;

    public RadarrApiCreateDownloadClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public async Task<Result<RadarrDownloadClientResourceDTO>> ExecuteAsync(
        RadarrApiCreateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/downloadclient?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Content = json.ToStringContent();

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to create download client in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var created = JsonSerializer.Deserialize<RadarrDownloadClientResourceDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new RadarrDownloadClientResourceDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
