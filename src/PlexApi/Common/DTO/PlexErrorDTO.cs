using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi;

public record PlexErrorDTO
{
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("status")]
    public required int Status { get; init; }
}
