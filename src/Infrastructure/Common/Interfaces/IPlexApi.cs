using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Infrastructure.Common.Models.OAuth;
using PlexRipper.Infrastructure.Common.Models.Plex;
using System;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Common.Interfaces
{
    public interface IPlexApi
    {
        Task<PlexStatus> GetStatus(string authToken, string uri);
        Task<PlexLibrariesForMachineId> GetLibrariesForMachineId(string authToken, string machineId);
        Task<PlexAuthentication> PlexSignInAsync(Account account);
        Task<PlexServerDTO> GetServer(string authToken);
        Task<PlexContainer> GetLibrarySections(string authToken, string plexFullHost);
        Task<PlexContainer> GetLibrary(string authToken, string plexFullHost, string libraryId);
        Task<PlexMetadata> GetEpisodeMetaData(string authToken, string host, int ratingKey);
        Task<PlexMetadata> GetMetadata(string authToken, string plexFullHost, int itemId);
        Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, int ratingKey);
        Task<PlexContainer> GetAllEpisodes(string authToken, string host, string section, int start, int retCount);
        Task<PlexFriends> GetUsers(string authToken);
        Task<PlexAccount> GetAccount(string authToken);
        Task<PlexMetadata> GetRecentlyAdded(string authToken, string uri, string sectionId);
        Task<OAuthPin> GetPin(int pinId);
        Uri GetOAuthUrl(string code, string applicationUrl);
        Task<PlexAddWrapper> AddUser(string emailAddress, string serverId, string authToken, int[] libs);

        /// <summary>
        /// Returns a new AuthToken and will update the <see cref="PlexAccount"/> in the DB.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<string> RefreshPlexAuthTokenAsync(Account account);
    }
}
