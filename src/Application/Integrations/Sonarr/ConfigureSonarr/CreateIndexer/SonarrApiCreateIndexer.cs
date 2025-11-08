using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;

namespace Reaparr.Application;

public record SonarrApiCreateIndexerCommand() : ICommand<Result<SonarrCreateIndexerDTO>>
{
    public required bool ForceSave { get; init; }

    public required SonarrCreateIndexerDTO Resource { get; init; }
}

public class SonarrApiCreateIndexerCommandHandler
    : ICommandHandler<SonarrApiCreateIndexerCommand, Result<SonarrCreateIndexerDTO>>
{
    private readonly HttpClient _client;

    public SonarrApiCreateIndexerCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrCreateIndexerDTO>> ExecuteAsync(
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

            var created = JsonSerializer.Deserialize<SonarrCreateIndexerDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(created ?? new SonarrCreateIndexerDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

public sealed class SonarrCreateIndexerDTO
{
    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("downloadClientId")]
    public int DownloadClientId { get; set; }

    [JsonPropertyName("enableRss")]
    public bool EnableRss { get; set; }

    [JsonPropertyName("enableAutomaticSearch")]
    public bool EnableAutomaticSearch { get; set; }

    [JsonPropertyName("enableInteractiveSearch")]
    public bool EnableInteractiveSearch { get; set; }

    [JsonPropertyName("supportsRss")]
    public bool SupportsRss { get; set; }

    [JsonPropertyName("supportsSearch")]
    public bool SupportsSearch { get; set; }

    [JsonPropertyName("fields")]
    public List<SonarrCreateFieldDTO>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public SonarrCreateIndexerMessageDTO? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // The API shows a circular reference here; keep it loosely typed
    [JsonPropertyName("presets")]
    public List<object>? Presets { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    // The API expects lowercase strings: unknown | usenet | torrent
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("seasonSearchMaximumSingleEpisodeAge")]
    public int SeasonSearchMaximumSingleEpisodeAge { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }
}

public sealed class SonarrCreateIndexerMessageDTO
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // The API expects lowercase strings: info | warning | error
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
