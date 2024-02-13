using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexLibraryService
{
    /// <summary>
    /// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
    /// </summary>
    /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
    /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
    ///  <returns>If successful.</returns>
    Task<Result> RetrieveAccessibleLibrariesAsync(int plexAccountId, int plexServerId);

    /// <summary>
    /// Returns the PlexLibrary by the Id, will refresh if the library has no media assigned.
    /// Note: this will not include the media.
    /// </summary>
    /// <param name="libraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
    /// <returns>Valid result if found.</returns>
    Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId);

    Task<Result> RetrieveAccessibleLibrariesForAllAccountsAsync(int plexServerId);
    Task<Result> RetrieveAllAccessibleLibrariesAsync(int plexAccountId);
}