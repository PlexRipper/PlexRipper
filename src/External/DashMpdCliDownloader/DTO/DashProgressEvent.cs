using System.Text.Json.Serialization;

namespace Reaparr.External;

public sealed record DashProgressEvent
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("percent")]
    public int Percent { get; init; }

    [JsonPropertyName("bandwidth")]
    public long Bandwidth { get; init; }

    [JsonPropertyName("eta_seconds")]
    public int? EtaSeconds { get; init; }

    [JsonPropertyName("downloaded_bytes")]
    public long DownloadedBytes { get; init; }

    [JsonPropertyName("estimated_total_bytes")]
    public long? TotalBytes { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonIgnore]
    public bool IsProgress => string.Equals(Type, "progress", StringComparison.OrdinalIgnoreCase);
}
