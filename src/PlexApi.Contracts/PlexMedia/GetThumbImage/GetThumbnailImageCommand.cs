using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetThumbnailImageCommand : ICommand<Result<byte[]>>
{
    public required int PlexServerId { get; init; }
    public required string PlexKey { get; init; }
    public required int MetaDataKey { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
