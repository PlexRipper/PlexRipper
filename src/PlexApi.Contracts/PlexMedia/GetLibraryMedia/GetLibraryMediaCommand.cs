using FastEndpoints;

namespace Reaparr.PlexApi.Contracts.GetLibraryMedia;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress>? Action = null)
    : ICommand<Result<LibraryMetadata>>;
