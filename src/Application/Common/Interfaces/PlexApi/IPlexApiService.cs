using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexApiService
    {
        #region Methods

        /// <summary>
        ///     Returns the <see cref="PlexAccount" /> after PlexApi validation.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<PlexAccount> PlexSignInAsync(string username, string password);

        Task<string> RefreshPlexAuthTokenAsync(PlexAccount account);

        Task<PlexAccount> GetAccountAsync(string authToken);

        /// <summary>
        /// Retrieves the accessible <see cref="PlexServer"/> by this plexAccountToken by sending an API request to the PlexAPI.
        /// </summary>
        /// <param name="plexAccountToken">The <see cref="PlexAccount"/> token to retrieve the accessible <see cref="PlexServer"/>s with.</param>
        /// <returns>The accessible <see cref="PlexServer"/>s.</returns>
        Task<List<PlexServer>> GetServersAsync(string plexAccountToken);

        Task<List<PlexLibrary>> GetLibrarySectionsAsync(string authToken, string plexServerBaseUrl);

        /// <summary>
        ///     Returns and PlexLibrary container with either Movies, Series, Music or Photos depending on the type.
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <param name="authToken"></param>
        /// <param name="plexServerBaseUrl"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary plexLibrary, string authToken);

        Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl);

        Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey);

        Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl);

        Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow);

        Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken, int width = 0, int height = 0);

        Task<byte[]> GetBannerAsync(string bannerUrl, string authToken, int width = 0, int height = 0);
        #endregion

        Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(string serverAuthToken, string plexFullHost, string
            plexLibraryKey);

        Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(string serverAuthToken, string plexFullHost,
            string plexLibraryKey);

    }
}