namespace Reaparr.Application.Contracts;

/// <summary>
/// Retrieve the accessible <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database. The <see cref="PlexServer"/> in question will need to be online.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to retrieve the accessible <see cref="PlexLibrary">Plex Libraries</see> for.</param>
/// <param name="PlexServerId">The id of the <see cref="PlexServer"/> to retrieve <see cref="PlexLibrary">Plex Libraries</see> for.</param>
///  <returns>If successful.</returns>
public record RefreshLibraryAccessCommand(int PlexAccountId, int PlexServerId = 0)
    : ICommand<Result<PlexLibraryAccessRefreshResponse>>;
