using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress> Action)
    : ICommand<Result<LibraryMetadata>>;
