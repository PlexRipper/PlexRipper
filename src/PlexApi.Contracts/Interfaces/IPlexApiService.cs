using Application.Contracts;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public interface IPlexApiService
{
    /// <summary>
    ///     Returns the <see cref="PlexAccount" /> after PlexApi validation.
    /// </summary>
    /// <returns></returns>
    Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount);

    /// <summary>
    /// Retrieves the accessible <see cref="PlexServer">PlexServers</see> by this <see cref="PlexAccount"/> server token.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <returns>Returns the list of <see cref="PlexServer">PlexServers</see> this <see cref="PlexAccount"/> has access too
    /// and a separate list of tokens this account has to use to communicate with the <see cref="PlexServer"/></returns>
    public Task<(
        Result<List<PlexServer>> servers,
        Result<List<ServerAccessTokenDTO>> tokens
    )> GetAccessiblePlexServersAsync(int plexAccountId);

    /// <summary>
    /// Retrieves all accessible <see cref="PlexLibrary"/> from this <see cref="PlexServer"/> by the given <see cref="PlexAccount"/>.
    /// </summary>
    /// <param name="plexServerId"> The <see cref="PlexServer"/> to use.</param>
    /// <param name="plexAccountId"> The <see cref="PlexAccount"/> to use.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns>List of accessible <see cref="PlexLibrary"/>.</returns>
    Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(
        int plexServerId,
        int plexAccountId = 0,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches the PlexLibrary container with either Movies, Series, Music or Photos media depending on the type.
    /// Id and PlexServerId are copied over from the input parameter.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the media from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<PlexLibrary>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Attempts to connect to a server by the given <see cref="PlexServerConnection"/> and returns the <see cref="PlexServerStatus"/> based on the result.
    /// </summary>
    /// <param name="plexServerConnectionId">The <see cref="PlexServerConnection"/> to test for. </param>
    /// <param name="action">Progress action callback to notify of connection attempt progress.</param>
    /// <returns>The Result is successful if the <see cref="PlexServerStatus"/> was created successfully, regardless of whether the connection was successful.</returns>
    Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(
        int plexServerConnectionId = 0,
        Action<PlexApiClientProgress>? action = null
    );

    /// <summary>
    /// Fetches all the <see cref="PlexTvShowSeason">Plex TvShow Seasons</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the seasons from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches all the <see cref="PlexTvShowEpisode">Plex TvShow Episodes</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the episodes from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );

    Task<Result<PlexAccount>> ValidatePlexToken(PlexAccount plexAccount);
}
