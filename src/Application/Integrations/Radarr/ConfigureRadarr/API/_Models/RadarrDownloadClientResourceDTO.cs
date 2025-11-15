using System.Text.Json.Serialization;

namespace Reaparr.Application;

public sealed class RadarrDownloadClientResourceDTO
{
    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("fields")]
    public List<RadarrDownloadClientResourceFieldDTO>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public RadarrProviderMessageDTO? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("presets")]
    public List<RadarrDownloadClientResourceDTO>? Presets { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("removeCompletedDownloads")]
    public bool RemoveCompletedDownloads { get; set; }

    [JsonPropertyName("removeFailedDownloads")]
    public bool RemoveFailedDownloads { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }
}

public sealed class RadarrProviderMessageDTO
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

// protocol/type enums are represented as strings in Radarr OpenAPI; using string? above

public sealed class RadarrDownloadClientResourceFieldDTO
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("helpText")]
    public string? HelpText { get; set; }

    [JsonPropertyName("helpTextWarning")]
    public string? HelpTextWarning { get; set; }

    [JsonPropertyName("helpLink")]
    public string? HelpLink { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("advanced")]
    public bool Advanced { get; set; }

    [JsonPropertyName("selectOptions")]
    public List<RadarrSelectOptionDTO>? SelectOptions { get; set; }

    [JsonPropertyName("selectOptionsProviderAction")]
    public string? SelectOptionsProviderAction { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("hidden")]
    public string? Hidden { get; set; }

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("isFloat")]
    public bool IsFloat { get; set; }
}

public sealed class RadarrSelectOptionDTO
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("hint")]
    public string? Hint { get; set; }

    [JsonPropertyName("dividerAfter")]
    public bool DividerAfter { get; set; }
}
