using System.Text.Json.Serialization;
using PlexRipper.Domain;

namespace Application.Contracts;

public record FileMergeJobUpdateDTO
{
    [JsonPropertyName("id")]
    public required DownloadTaskKey DownloadTaskId { get; init; }
}
