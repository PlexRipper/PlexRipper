using System.Text.Json.Serialization;

namespace Reaparr.Application;

// Mirrors Radarr IndexerResource (response) shape
public sealed class RadarrIndexerResourceDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("fields")]
    public List<RadarrIndexerResourceFieldDTO>? Fields { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public RadarrProviderMessageDTO? Message { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }

    [JsonPropertyName("presets")]
    public List<RadarrIndexerResourceDTO>? Presets { get; set; }

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

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("downloadClientId")]
    public int DownloadClientId { get; set; }
}

public sealed class RadarrIndexerResourceFieldDTO
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
