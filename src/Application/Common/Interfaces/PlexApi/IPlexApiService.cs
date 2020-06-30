using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.PlexApi
{
    public interface
        IPlexApiService
    {
        Task<PlexAccount> PlexSignInAsync(string username, string password);
        Task<string> RefreshPlexAuthTokenAsync(Account account);
        Task<PlexAccount> GetAccountAsync(string authToken);
        Task<List<PlexServer>> GetServerAsync(string authToken);
        Task<List<PlexLibrary>> GetLibrarySectionsAsync(string authToken, string plexLibraryUrl);

        /// <summary>
        /// Returns and PlexLibrary container with either Movies, Series, Music or Photos depending on the type. 
        /// </summary>
        /// <param name="library"></param>
        /// <param name="authToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary library, string authToken, string plexFullHost);

        Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl);
        Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey);
        Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl);
    }


}
