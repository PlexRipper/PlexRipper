using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Func<MediaSyncProgress, Task> Action)
    : ICommand<Result<LibraryMetadata>>;
