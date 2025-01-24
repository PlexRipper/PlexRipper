using System.Text.Json.Serialization;

namespace Application.Contracts;

public record CheckAllConnectionStatusUpdateDTO
{
    [JsonPropertyName("plexServersWithConnectionIds")]
    public required Dictionary<int, List<int>> PlexServersWithConnectionIds { get; init; } = new();
}
