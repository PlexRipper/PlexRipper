namespace Reaparr.Application.Contracts;

/// <summary>
/// Compares a remote TV show library against an owned TV show library.
/// Produces show, season, and episode comparison hit rows plus a scope row.
/// </summary>
public record CompareTvShowPlexLibraryCommand(
    int RemotePlexLibraryId,
    int OwnedPlexLibraryId
) : ICommand<Result>;
