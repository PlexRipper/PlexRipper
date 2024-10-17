using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi;

public class PlexErrorsResponseDTO
{
    [JsonPropertyName("errors")]
    public List<PlexErrorDTO> Errors { get; set; } = new();
}
