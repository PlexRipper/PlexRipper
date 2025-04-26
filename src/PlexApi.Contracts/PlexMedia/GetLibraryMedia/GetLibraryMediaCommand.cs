using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts.GetLibraryMedia;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress>? Action = null)
    : ICommand<Result<LibraryMetadata>>;
