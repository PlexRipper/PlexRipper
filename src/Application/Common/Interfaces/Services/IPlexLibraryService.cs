namespace PlexRipper.Application;

public interface IPlexLibraryService
{
    /// <summary>
    /// Retrieves the new media metadata from the PlexApi and stores it in the database.
    /// </summary>
    /// <param name="plexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
    /// <param name="progressAction">The action to call for a progress update.</param>
    /// <returns>Returns the PlexLibrary with the containing media.</returns>
    Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexLibraryId, Action<LibraryProgress> progressAction = null);

    /// <summary>
    /// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
    /// </summary>
    /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
    /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
    ///  <returns>If successful.</returns>
    Task<Result> RetrieveAccessibleLibrariesAsync(int plexAccountId, int plexServerId);

    /// <summary>
    /// Returns the PlexLibrary by the Id, will refresh if the library has no media assigned.
    /// </summary>
    /// <param name="libraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
    /// <param name="topLevelMediaOnly"></param>
    /// <returns>Valid result if found.</returns>
    Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, bool topLevelMediaOnly = false);

    Task<Result<PlexServer>> GetPlexLibraryInServerAsync(int libraryId, bool topLevelMediaOnly = false);

    Task<Result<List<PlexLibrary>>> GetAllPlexLibrariesAsync();

    Task<Result> UpdateDefaultDestinationLibrary(int libraryId, int folderPathId);
    Task<Result> RetrieveAccessibleLibrariesForAllAccountsAsync(int plexServerId);
    Task<Result> RetrieveAllAccessibleLibrariesAsync(int plexAccountId);
}