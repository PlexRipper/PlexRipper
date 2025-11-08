using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record SonarrApiUpdateIndexerCommand : ICommand<Result<SonarrUpdateIndexerDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrUpdateIndexerDTO Resource { get; init; }
}

public class SonarrApiUpdateIndexerCommandValidator : Validator<SonarrApiUpdateIndexerCommand>
{
    public SonarrApiUpdateIndexerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarrApiUpdateIndexerCommandHandler
    : ICommandHandler<SonarrApiUpdateIndexerCommand, Result<SonarrUpdateIndexerDTO>>
{
    private readonly HttpClient _client;

    public SonarrApiUpdateIndexerCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrUpdateIndexerDTO>> ExecuteAsync(
        SonarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update indexer in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<SonarrUpdateIndexerDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new SonarrUpdateIndexerDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

public sealed class SonarrUpdateIndexerDTO
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
    public List<SonarrUpdateFieldDTO>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public SonarrUpdateIndexerMessageDto? Message { get; set; }

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

public sealed class SonarrUpdateIndexerMessageDto
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // The API expects lowercase strings: info | warning | error
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
