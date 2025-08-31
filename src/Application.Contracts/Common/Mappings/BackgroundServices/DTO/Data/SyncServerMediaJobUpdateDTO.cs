using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

public record SyncServerMediaJobUpdateDTO
{
    [JsonPropertyName("plexServerId")]
    public required int PlexServerId { get; init; }

    [JsonPropertyName("forceSync")]
    public required bool ForceSync { get; init; }
}
