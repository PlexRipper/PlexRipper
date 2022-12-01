namespace PlexRipper.Application;

public interface IPlexApiService
{
    #region Methods

    /// <summary>
    ///     Returns the <see cref="PlexAccount" /> after PlexApi validation.
    /// </summary>
    /// <returns></returns>
    Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount);

    Task<string> RefreshPlexAuthTokenAsync(PlexAccount account);

    Task<PlexAccount> GetAccountAsync(string authToken);


    /// <summary>
    /// Retrieves the accessible <see cref="PlexServer">PlexServers</see> by this <see cref="PlexAccount"/> server token.
    /// </summary>
    /// <param name="plexAccount"></param>
    /// <returns>Returns the list of <see cref="PlexServer">PlexServers</see> this <see cref="PlexAccount"/> has access too
    /// and a separate list of tokens this account has to use to communicate with the <see cref="PlexServer"/></returns>
    public Task<(Result<List<PlexServer>> servers, Result<List<ServerAccessTokenDTO>> tokens)> GetServersAsync(PlexAccount plexAccount);

    Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(string authToken, string plexServerBaseUrl);

    /// <summary>
    ///     Returns and PlexLibrary container with either Movies, Series, Music or Photos depending on the type.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <param name="authToken"></param>
    /// <returns></returns>
    Task<Result<PlexLibrary>> GetLibraryMediaAsync(PlexLibrary plexLibrary, string authToken);

    Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl);

    Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey);

    Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl, Action<PlexApiClientProgress> action = null);

    Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow);

    Task<Result<byte[]>> GetPlexMediaImageAsync(string thumbUrl, string authToken, int width = 0, int height = 0);

    #endregion

    Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(string serverAuthToken, string plexFullHost, string plexLibraryKey);

    Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(string serverAuthToken, string plexFullHost, string plexLibraryKey);

    Task<Result<AuthPin>> Get2FAPin(string clientId);

    Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId);
}