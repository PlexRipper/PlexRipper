using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts.GetLibraryMedia;

public record GetLibraryMediaCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress>? Action = null)
    : ICommand<Result<LibraryMetadata>>;
