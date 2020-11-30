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

        Task<List<PlexServer>> GetServersAsync(string authToken);

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

        Task<List<PlexTvShowEpisode>> GetEpisodesAsync(string serverAuthToken, string plexFullHost, PlexTvShowSeason plexTvShowSeason);

        Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken, int width = 0, int height = 0);

        #endregion
    }
}