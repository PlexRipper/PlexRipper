using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

public record MoveDownloadFileJobUpdateDTO
{
    [JsonPropertyName("id")]
    public required DownloadTaskKey DownloadTaskId { get; init; }
}
