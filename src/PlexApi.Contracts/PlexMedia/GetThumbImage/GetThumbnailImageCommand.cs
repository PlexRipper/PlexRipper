using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetThumbnailImageCommand : ICommand<Result<ThumbnailImageResponse>>
{
    public required int PlexServerId { get; init; }
    public required int PlexKey { get; init; }
    public required int MetaDataKey { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
