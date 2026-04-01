namespace Reaparr.Application.Contracts;

public record DownloadJobUpdateDTO
{
    [JsonPropertyName("id")]
    public required DownloadTaskKey Id { get; init; }
}
