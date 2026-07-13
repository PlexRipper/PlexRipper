namespace Reaparr.Application.Contracts;

/// <summary>
/// Compares a remote library against an owned library for a specific media type.
/// Produces comparison hit rows and a scope row so browse queries can project
/// comparison state efficiently.
/// </summary>
public record CompareMoviePlexLibraryCommand(
    int RemotePlexLibraryId,
    int OwnedPlexLibraryId,
    PlexMediaType MediaType
) : ICommand<Result>;
