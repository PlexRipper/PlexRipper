using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;

namespace Reaparr.Application;

public record SonarrApiCreateIndexerCommand() : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required bool ForceSave { get; init; }

    public required SonarrIndexerContractDTO Resource { get; init; }
}

public class SonarrApiCreateIndexerCommandHandler
    : ICommandHandler<SonarrApiCreateIndexerCommand, Result<SonarrIndexerContractDTO>>
{
    private readonly HttpClient _client;

    public SonarrApiCreateIndexerCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiCreateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to create indexer in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var created = JsonSerializer.Deserialize<SonarrIndexerContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new SonarrIndexerContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
