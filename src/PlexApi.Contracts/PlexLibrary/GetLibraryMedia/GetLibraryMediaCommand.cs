using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress> Action)
    : ICommand<Result<LibraryMetadata>>;
