using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

public record InspectPlexServerJobUpdateDTO
{
    [JsonPropertyName("plexServerIds")]
    public required List<int> PlexServerIds { get; init; }
}
