using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record SonarApiUpdateDownloadClientCommand : ICommand<Result<SonarrUpdateDownloadClientDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrUpdateDownloadClientDTO Resource { get; init; }
}

public class SonarApiUpdateDownloadClientCommandValidator : Validator<SonarApiUpdateDownloadClientCommand>
{
    public SonarApiUpdateDownloadClientCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarApiUpdateDownloadClientCommandHandler
    : ICommandHandler<SonarApiUpdateDownloadClientCommand, Result<SonarrUpdateDownloadClientDTO>>
{
    private readonly HttpClient _client;

    public SonarApiUpdateDownloadClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrUpdateDownloadClientDTO>> ExecuteAsync(
        SonarApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var forceSave = command.ForceSave ? "true" : "false";
        var requestUri = new Uri($"/api/v3/downloadclient/{command.Id}?forceSave={forceSave}", UriKind.Relative);
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
                .Fail($"Failed to update download client in Sonarr. StatusCode: {response.StatusCode}")
                .WithError(body)
                .LogError();
        }

        var updated = JsonSerializer.Deserialize<SonarrUpdateDownloadClientDTO>(
            body,
            DefaultJsonSerializerOptions.ConfigStandard
        );

        return Result.Ok(updated ?? new SonarrUpdateDownloadClientDTO());
    }
}

public sealed class SonarrUpdateDownloadClientDTO
{
    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("fields")]
    public List<SonarrUpdateFieldDto>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public SonarrUpdateDownloadClientMessageDto? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Sonarr shows a circular reference here; keep it loosely typed
    [JsonPropertyName("presets")]
    public List<object>? Presets { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    // The API expects lowercase strings: unknown | usenet | torrent
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("removeCompletedDownloads")]
    public bool RemoveCompletedDownloads { get; set; }

    [JsonPropertyName("removeFailedDownloads")]
    public bool RemoveFailedDownloads { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }
}

public sealed class SonarrUpdateFieldDto
{
    [JsonPropertyName("advanced")]
    public bool Advanced { get; set; }

    [JsonPropertyName("helpLink")]
    public string? HelpLink { get; set; }

    [JsonPropertyName("helpText")]
    public string? HelpText { get; set; }

    [JsonPropertyName("helpTextWarning")]
    public string? HelpTextWarning { get; set; }

    [JsonPropertyName("hidden")]
    public string? Hidden { get; set; }

    [JsonPropertyName("isFloat")]
    public bool IsFloat { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    // The API shows lowercase strings like "normal", keep as string
    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("selectOptions")]
    public List<SonarrUpdateSelectOptionDto>? SelectOptions { get; set; }

    [JsonPropertyName("selectOptionsProviderAction")]
    public string? SelectOptionsProviderAction { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class SonarrUpdateSelectOptionDto
{
    [JsonPropertyName("hint")]
    public string? Hint { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public sealed class SonarrUpdateDownloadClientMessageDto
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // The API expects lowercase strings: info | warning | error
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
