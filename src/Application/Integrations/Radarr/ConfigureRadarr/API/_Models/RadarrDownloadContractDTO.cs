using System.Text.Json.Serialization;

namespace Reaparr.Application;

public sealed class RadarrDownloadContractDTO
{
    [JsonPropertyName("configContract")]
    public string? ConfigContract { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("fields")]
    public List<RadarrDownloadContractCreateFieldDTO>? Fields { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("implementation")]
    public string? Implementation { get; set; }

    [JsonPropertyName("implementationName")]
    public string? ImplementationName { get; set; }

    [JsonPropertyName("infoLink")]
    public string? InfoLink { get; set; }

    [JsonPropertyName("message")]
    public RadarrDownloadContractMessageDTO? Message { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Radarr shows a circular reference here; keep it loosely typed
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

public sealed class RadarrDownloadContractCreateFieldDTO
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

    // Keep value loosely typed to support ints/bools/strings/lists
    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class RadarrDownloadContractMessageDTO
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
