using System.Text.Json.Serialization;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record MoveDownloadFileJobUpdateDTO
{
    [JsonPropertyName("id")]
    public required DownloadTaskKey DownloadTaskId { get; init; }
}
