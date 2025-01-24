using System.Text.Json.Serialization;
using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadJobUpdateDTO
{
    [JsonPropertyName("id")]
    public required DownloadTaskKey Id { get; init; }
}
