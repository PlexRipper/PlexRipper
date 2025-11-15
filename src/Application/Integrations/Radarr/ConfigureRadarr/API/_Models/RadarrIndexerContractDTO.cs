using System.Text.Json.Serialization;

namespace Reaparr.Application;

public class RadarrIndexerContractDTO
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
    public List<RadarrIndexerContractFieldDTO>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public RadarrIndexerContractMessageDTO? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // The API expects lowercase strings: unknown | usenet | torrent
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; }
}

public sealed class RadarrIndexerContractFieldDTO
{
    [JsonPropertyName("advanced")]
    public bool Advanced { get; set; }

    [JsonPropertyName("hidden")]
    public string? Hidden { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("helpText")]
    public string? HelpText { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    // keep value loosely typed
    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class RadarrIndexerContractMessageDTO
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
