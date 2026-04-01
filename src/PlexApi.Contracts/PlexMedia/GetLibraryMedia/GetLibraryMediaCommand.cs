namespace Reaparr.PlexApi.Contracts.GetLibraryMedia;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Func<MediaSyncProgress, Task>? Action = null)
    : ICommand<Result<LibraryMetadata>>;
