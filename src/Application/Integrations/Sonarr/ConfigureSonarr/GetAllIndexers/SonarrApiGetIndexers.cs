namespace Reaparr.Application;

public record SonarrApiGetIndexersCommand(Guid IntegrationId) : ICommand<Result<List<IndexerResourceDTO>>>;

public class SonarrApiGetIndexersCommandHandler
    : ICommandHandler<SonarrApiGetIndexersCommand, Result<List<IndexerResourceDTO>>>
{
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarrApiGetIndexersCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<List<IndexerResourceDTO>>> ExecuteAsync(
        SonarrApiGetIndexersCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<List<IndexerResourceDTO>>();

        using var client = clientResult.Value;
        return await client.GetSonarrIndexersAsync(cancellationToken);
    }
}

public sealed class IndexerResourceDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public IndexerMessageDTO? Message { get; set; }

    [JsonPropertyName("enableRss")]
    public bool EnableRss { get; set; }

    [JsonPropertyName("enableAutomaticSearch")]
    public bool EnableAutomaticSearch { get; set; }

    [JsonPropertyName("enableInteractiveSearch")]
    public bool EnableInteractiveSearch { get; set; }

    [JsonPropertyName("fields")]
    public List<IndexerFieldDTO>? Fields { get; set; }

    // The API shows a circular reference here; keep it loosely typed
    [JsonPropertyName("presets")]
    public List<object>? Presets { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    // The API expects lowercase strings: unknown | usenet | torrent
    [JsonPropertyName("protocol")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IndexerProtocol Protocol { get; set; }

    [JsonPropertyName("seasonSearchMaximumSingleEpisodeAge")]
    public int SeasonSearchMaximumSingleEpisodeAge { get; set; }

    [JsonPropertyName("supportsRss")]
    public bool SupportsRss { get; set; }

    [JsonPropertyName("supportsSearch")]
    public bool SupportsSearch { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("downloadClientId")]
    public int DownloadClientId { get; set; }
}

public sealed class IndexerFieldDTO
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
    public List<IndexerSelectOptionDTO>? SelectOptions { get; set; }

    [JsonPropertyName("selectOptionsProviderAction")]
    public string? SelectOptionsProviderAction { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class IndexerSelectOptionDTO
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

public sealed class IndexerMessageDTO
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    // The API expects lowercase strings: info | warning | error
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IndexerMessageType Type { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexerMessageType
{
    Info,
    Warning,
    Error,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexerProtocol
{
    Unknown,
    Usenet,
    Torrent,
}
