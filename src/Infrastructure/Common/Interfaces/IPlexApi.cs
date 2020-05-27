using PlexRipper.Application.Common.DTO.Plex;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.DTO.PlexGetLibrarySections;
using PlexRipper.Infrastructure.Common.DTO.PlexGetServer;
using PlexRipper.Infrastructure.Common.DTO.PlexGetStatus;
using PlexRipper.Infrastructure.Common.DTO.PlexLibrary;
using PlexRipper.Infrastructure.Common.DTO.PlexLibraryMedia;
using PlexRipper.Infrastructure.Common.Models.OAuth;
using System;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Common.Interfaces
{
    public interface IPlexApi
    {
        Task<PlexStatusDTO> GetStatus(string authToken, string uri);
        Task<PlexLibrariesForMachineId> GetLibrariesForMachineId(string authToken, string machineId);
        Task<PlexAuthenticationDTO> PlexSignInAsync(string username, string password);
        Task<PlexServerContainerXML> GetServer(string authToken);
        Task<PlexLibrarySectionsDTO> GetLibrarySections(string plexAuthToken, string plexFullHost);
        Task<PlexLibraryMediaDTO> GetLibraryMediaAsync(string authToken, string plexFullHost, string libraryId);
        Task<PlexMetadata> GetEpisodeMetaData(string authToken, string host, int ratingKey);
        Task<PlexMediaMetaDataDTO> GetMetadata(string authToken, string plexFullHost, int itemId);
        Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, int ratingKey);
        Task<PlexLibraryContainerDTO> GetAllEpisodes(string authToken, string host, string section, int start, int retCount);
        Task<PlexFriendsXML> GetUsers(string authToken);
        Task<PlexAccount> GetAccount(string authToken);
        Task<PlexMetadata> GetRecentlyAdded(string authToken, string uri, string sectionId);
        Task<OAuthPin> GetPin(int pinId);
        Uri GetOAuthUrl(string code, string applicationUrl);
        Task<PlexAddWrapper> AddUser(string emailAddress, string serverId, string authToken, int[] libs);

        Task<string> RefreshPlexAuthTokenAsync(Account account);

        bool DownloadMedia(string authToken, string downloadUrl, string fileName);
        string GetDownloadUrl(PlexServer server, PlexMediaMetaDataDTO metaDataDto);
        string GetDownloadFilename(PlexServer server, PlexMediaMetaDataDTO metaDataDto);
    }
}
