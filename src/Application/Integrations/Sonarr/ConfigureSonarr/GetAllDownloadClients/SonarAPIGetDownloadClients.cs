namespace Reaparr.Application;

public record SonarApiGetDownloadClientsCommand(Guid IntegrationId) : ICommand<Result<List<DownloadClientResourceDTO>>>;

public class SonarApiGetDownloadClientsCommandHandler
    : ICommandHandler<SonarApiGetDownloadClientsCommand, Result<List<DownloadClientResourceDTO>>>
{
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarApiGetDownloadClientsCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<List<DownloadClientResourceDTO>>> ExecuteAsync(
        SonarApiGetDownloadClientsCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<List<DownloadClientResourceDTO>>();

        using var client = clientResult.Value;
        return await client.GetSonarrDownloadClientsAsync(cancellationToken);
    }
}

public sealed class DownloadClientResourceDTO
{
    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("fields")]
    public List<Field>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public DownloadClientMessage? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("presets")]
    public List<DownloadClientResourceDTO>? Presets { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("protocol")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DownloadClientProtocol Protocol { get; set; }

    [JsonPropertyName("removeCompletedDownloads")]
    public bool RemoveCompletedDownloads { get; set; }

    [JsonPropertyName("removeFailedDownloads")]
    public bool RemoveFailedDownloads { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }
}

public sealed class Field
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

    [JsonPropertyName("privacy")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FieldPrivacy Privacy { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("selectOptions")]
    public List<SelectOption>? SelectOptions { get; set; }

    [JsonPropertyName("selectOptionsProviderAction")]
    public string? SelectOptionsProviderAction { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class SelectOption
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

public sealed class DownloadClientMessage
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType Type { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    Info,
    Warning,
    Error,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadClientProtocol
{
    Unknown,
    Usenet,
    Torrent,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FieldPrivacy
{
    Normal,
    Password,
    ApiKey,
    UserName,
}
