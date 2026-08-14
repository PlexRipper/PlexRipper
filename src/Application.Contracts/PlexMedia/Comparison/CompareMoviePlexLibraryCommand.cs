namespace Reaparr.Application.Contracts;

/// <summary>
/// Compares a remote movie library against an owned movie library.
/// Produces comparison hit rows and a scope row so browse queries can project
/// movie comparison state efficiently.
/// </summary>
public record CompareMoviePlexLibraryCommand(int OwnedPlexLibraryId, int RemotePlexLibraryId) : ICommand<Result>;
