namespace Reaparr.PlexApi.Contracts;

public record ThumbnailImageResponse
{
    public required byte[] Data { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
}
