using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

public record CheckAllConnectionStatusUpdateDTO
{
    [JsonPropertyName("plexServersWithConnectionIds")]
    public required Dictionary<int, List<int>> PlexServersWithConnectionIds { get; init; } = new();
}
