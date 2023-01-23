using Application.Contracts;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public interface IPlexApiService
{
    #region Methods

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
    public Task<(Result<List<PlexServer>> servers, Result<List<ServerAccessTokenDTO>> tokens)> GetAccessiblePlexServersAsync(int plexAccountId);

    /// <summary>
    /// Retrieves all accessible <see cref="PlexLibrary"/> from this <see cref="PlexServer"/> by the given <see cref="PlexAccount"/>.
    /// </summary>
    /// <param name="plexServerId"></param>
    /// <param name="plexAccountId"></param>
    /// <returns>List of accessible <see cref="PlexLibrary"/>.</returns>
    Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(int plexServerId, int plexAccountId = 0);

    /// <summary>
    /// Fetches the PlexLibrary container with either Movies, Series, Music or Photos media depending on the type.
    /// Id and PlexServerId are copied over from the input parameter.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <param name="plexAccount">The optional PlexAccount used to connect to the <see cref="PlexServer"/> </param>
    /// <returns></returns>
    Task<Result<PlexLibrary>> GetLibraryMediaAsync(PlexLibrary plexLibrary, PlexAccount plexAccount = null);

    Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl);

    Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey);

    /// <summary>
    /// Attempts to connect to a server by the given <see cref="PlexServerConnection"/> and returns the <see cref="PlexServerStatus"/> based on the result.
    /// </summary>
    /// <param name="plexServerConnectionId">The <see cref="PlexServerConnection"/> to test for. </param>
    /// <param name="action">Progress action callback to notify of connection attempt progress.</param>
    /// <returns>The Result is successful if the <see cref="PlexServerStatus"/> was created successfully, regardless of whether the connection was successful.</returns>
    Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(int plexServerConnectionId = 0, Action<PlexApiClientProgress> action = null);

    Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow);

    /// <summary>
    /// Retrieves the media image from the <see cref="PlexServer"/> with optional dimensions.
    /// </summary>
    /// <param name="plexServer">The <see cref="PlexServer"/> to request the media from.</param>
    /// <param name="thumbPath">The relative media url</param>
    /// <param name="width">The optional width of the image.</param>
    /// <param name="height">The optional height of the image.</param>
    /// <returns></returns>
    Task<Result<byte[]>> GetPlexMediaImageAsync(PlexServer plexServer, string thumbPath, int width = 0, int height = 0);

    #endregion


    /// <summary>
    /// Fetches all the <see cref="PlexTvShowSeason">Plex TvShow Seasons</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(PlexLibrary plexLibrary);

    /// <summary>
    /// Fetches all the <see cref="PlexTvShowEpisode">Plex TvShow Episodes</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(PlexLibrary plexLibrary);

    Task<Result<AuthPin>> Get2FAPin(string clientId);

    Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId);


    Task<Result<string>> GetPlexServerTokenWithUrl(int plexServerId, string serverUrl, int plexAccountId = 0);
}