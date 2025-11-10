using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;

namespace Reaparr.Application;

public record SonarrApiCreateDownloadClientCommand() : ICommand<Result<SonarrDownloadContractDTO>>
{
    public required bool ForceSave { get; init; }

    public required SonarrDownloadContractDTO Resource { get; init; }
}

public class SonarApiCreateDownloadClientCommandHandler
    : ICommandHandler<SonarrApiCreateDownloadClientCommand, Result<SonarrDownloadContractDTO>>
{
    private readonly HttpClient _client;

    public SonarApiCreateDownloadClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrDownloadContractDTO>> ExecuteAsync(
        SonarrApiCreateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/downloadclient?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to create download client in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var created = JsonSerializer.Deserialize<SonarrDownloadContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new SonarrDownloadContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

