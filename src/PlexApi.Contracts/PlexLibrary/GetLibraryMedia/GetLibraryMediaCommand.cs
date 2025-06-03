using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress> Action)
    : ICommand<Result<LibraryMetadata>>;
