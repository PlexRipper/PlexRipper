using PlexRipper.Domain.Entities;
using PlexRipper.PlexApi.Common.DTO;
using PlexRipper.PlexApi.Common.DTO.PlexGetLibrarySections;
using PlexRipper.PlexApi.Common.DTO.PlexGetServer;
using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using PlexRipper.PlexApi.Common.DTO.PlexLibrary;
using PlexRipper.PlexApi.Common.DTO.PlexLibraryMedia;
using PlexRipper.PlexApi.Common.DTO.PlexSignIn;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.PlexApi.Api
{
    public class PlexApi
    {
        public PlexApi(PlexWebClient client, Serilog.ILogger logger)
        {
            Log = logger;
            Client = client;
        }

        public PlexWebClient Client { get; }
        private Serilog.ILogger Log { get; }

        private const string SignInUri = "https://plex.tv/users/sign_in.json";
        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string ServerUri = "https://plex.tv/pms/servers.xml";

        /// <summary>
        /// Sign into the Plex API
        /// This is for authenticating users credentials with Plex
        /// <para>NOTE: Plex "Managed" users do not work</para>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<PlexAuthenticationDTO> PlexSignInAsync(string username, string password)
        {
            var userModel = new PlexUserRequestDTO
            {
                User = new UserRequestDTO
                {
                    Login = username,
                    Password = password
                }
            };
            var request = new RestRequest(new Uri(SignInUri), Method.POST);
            request.AddJsonBody(userModel);
            return Client.SendRequestAsync<PlexAuthenticationDTO>(request);
        }


        public async Task<string> RefreshPlexAuthTokenAsync(Account account)
        {
            var result = await PlexSignInAsync(account.Username, account.Password);
            if (result != null)
            {
                Log.Information($"Returned token was: {result.User.AuthToken}");
                return result.User.AuthToken;
            }
            Log.Error("Result from RequestPlexSignInDataAsync() was null.");
            return string.Empty;
        }

        public async Task<PlexServerStatus> GetServerStatusAsync(string authToken, string serverBaseUrl)
        {
            var request = new RestRequest(new Uri(serverBaseUrl), Method.GET);
            request = AddToken(request, authToken);
            var response = await Client.SendRequestAsync(request);
            var status = new PlexServerStatus
            {
                StatusCode = (int)response.StatusCode,
                IsSuccessful = response.IsSuccessful,
                StatusMessage = response.IsSuccessful ? response.StatusDescription : response.ErrorMessage,
                LastChecked = DateTime.Now.ToUniversalTime()
            };
            return status;
        }

        public Task<PlexAccountDTO> GetAccountAsync(string authToken)
        {
            var request = new RestRequest(new Uri(GetAccountUri), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexAccountDTO>(request);
        }

        public Task<PlexServerContainerXML> GetServerAsync(string authToken)
        {
            var request = new RestRequest(new Uri(ServerUri), Method.GET, DataFormat.Xml);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexServerContainerXML>(request);
        }

        /// <summary>
        /// Returns an detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
        /// </summary>
        /// <param name="plexAuthToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        public Task<PlexLibrarySectionsDTO> GetLibrarySectionsAsync(string plexAuthToken, string plexFullHost)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections"), Method.GET);
            request = AddToken(request, plexAuthToken);
            return Client.SendRequestAsync<PlexLibrarySectionsDTO>(request);
        }

        public Task<PlexLibraryMediaDTO> GetLibraryMediaAsync(string authToken, string plexFullHost, string libraryId)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections/{libraryId}/all"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexLibraryMediaDTO>(request);
        }

        public Task<PlexLibrariesForMachineId> GetLibrariesForMachineIdAsync(string authToken, string machineId)
        {
            var request = new RestRequest(new Uri($"https://plex.tv/api/servers/{machineId}"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexLibrariesForMachineId>(request);
        }

        public Task<PlexMediaMetaDataDTO> GetMetadataAsync(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaMetaDataDTO>(request);
        }

        public Task<PlexMediaMetaDataDTO> GetMetadataAsync(string authToken, string metaDataUrl)
        {
            var request = new RestRequest(new Uri(metaDataUrl), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaMetaDataDTO>(request);
        }


        public string GetDownloadUrl(PlexServer server, PlexMediaMetaDataDTO metaDataDto)
        {
            try
            {
                string key = metaDataDto.MediaContainerDto.Metadata.First().Media.First().Part.First().Key;
                return $"{server.BaseUrl}{key}";
            }
            catch (Exception e)
            {
                Log.Error("Could not retrieve Download Url from MetaData", e);
            }
            return string.Empty;
        }

        public string GetDownloadFilename(PlexServer server, PlexMediaMetaDataDTO metaDataDto)
        {
            try
            {
                string path = metaDataDto.MediaContainerDto.Metadata.First().Media.First().Part.First().File;
                return Path.GetFileName(path);
            }
            catch (Exception e)
            {
                Log.Error("Could not retrieve Filename from MetaData", e);
            }
            return string.Empty;
        }


        public Task<PlexMetadataDTO> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMetadataDTO>(request);
        }

        /// <summary>
        /// Gets all episodes.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="host">The host.</param>
        /// <param name="section">The section.</param>
        /// <param name="start">The start count.</param>
        /// <param name="retCount">The return count, how many items you want returned.</param>
        /// <returns></returns>
        public Task<PlexLibraryContainerDTO> GetAllEpisodesAsync(string authToken, string plexFullHost, string section, int start, int retCount)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections/{section}/all"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, start, retCount);
            request.AddQueryParameter("type", "4");
            return Client.SendRequestAsync<PlexLibraryContainerDTO>(request);
        }

        /// <summary>
        /// Retuns all the Plex users for this account
        /// NOTE: For HOME USERS. There is no username or email, the user's home name is under the title property
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public Task<PlexFriendsXML> GetUsers(string authToken)
        {
            var request = new RestRequest(new Uri(FriendsUri), Method.GET, DataFormat.Xml);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexFriendsXML>(request);
        }

        public Task<PlexMetadataDTO> GetRecentlyAdded(string authToken, string hostUrl, string sectionId)
        {
            var request = new RestRequest(new Uri($"{hostUrl}/library/sections/{sectionId}/recentlyAdded"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, 0, 50);
            return Client.SendRequestAsync<PlexMetadataDTO>(request);
        }

        /// <summary>
        /// Adds the required headers and also the authorization header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authToken"></param>
        private RestRequest AddToken(RestRequest request, string authToken)
        {
            request.AddHeader("X-Plex-Token", authToken);
            return request;
        }


        private RestRequest AddLimitHeaders(RestRequest request, int from, int to)
        {
            request.AddHeader("X-Plex-Container-Start", from.ToString());
            request.AddHeader("X-Plex-Container-Size", to.ToString());
            return request;
        }


    }
}
