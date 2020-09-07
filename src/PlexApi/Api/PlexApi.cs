using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;
using PlexRipper.PlexApi.Models.Friends;
using PlexRipper.PlexApi.Models.Server;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataFormat = RestSharp.DataFormat;
using PlexAccount = PlexRipper.Domain.PlexAccount;

namespace PlexRipper.PlexApi.Api
{
    public class PlexApi
    {
        public PlexApi(PlexApiClient client)
        {
            Client = client;
        }

        public PlexApiClient Client { get; }

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
        public Task<PlexAccountDTO> PlexSignInAsync(string username, string password)
        {
            var userModel = new PlexUserRequest
            {
                User = new UserRequest
                {
                    Login = username,
                    Password = password
                }
            };
            var request = new RestRequest(new Uri(SignInUri), Method.POST);
            request.AddJsonBody(userModel);
            return Client.SendRequestAsync<PlexAccountDTO>(request);
        }


        public async Task<string> RefreshPlexAuthTokenAsync(PlexAccount plexAccount)
        {
            var result = await PlexSignInAsync(plexAccount.Username, plexAccount.Password);
            if (result != null)
            {
                Log.Information($"Returned token was: {result.User.AuthenticationToken}");
                return result.User.AuthenticationToken;
            }

            Log.Error("Result from RequestPlexSignInDataAsync() was null.");
            return string.Empty;
        }

        public async Task<PlexServerStatus> GetServerStatusAsync(string authToken, string serverBaseUrl)
        {
            // TODO Use healthCheck from here:
            // https://github.com/Arcanemagus/plex-api/wiki/Plex-Web-API-Overview
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

        public async Task<List<Server>> GetServerAsync(string authToken)
        {
            var request = new RestRequest(new Uri(ServerUri), Method.GET, DataFormat.Xml);
            request = AddToken(request, authToken);
            var serverContainer = await Client.SendRequestAsync<ServerContainer>(request);
            return serverContainer?.Servers;
        }

        /// <summary>
        /// Returns an detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
        /// </summary>
        /// <param name="plexAuthToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        public Task<PlexMediaContainer> GetLibrarySectionsAsync(string plexAuthToken, string plexFullHost)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections"), Method.GET);
            request = AddToken(request, plexAuthToken);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        public Task<PlexMediaContainer> GetMetadataForLibraryAsync(string authToken, string plexFullHost, string libraryId)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections/{libraryId}/all"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        public Task<PlexMediaContainer> GetMetadataAsync(string authToken, string plexFullHost, int metadataId)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{metadataId}"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        public Task<PlexMediaContainer> GetMetadataAsync(string authToken, string metaDataUrl)
        {
            var request = new RestRequest(new Uri(metaDataUrl), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }


        public Task<PlexMediaContainer> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"), Method.GET);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        /// <summary>
        /// Gets all episodes.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="host">The host.</param>
        /// <param name="plexFullHost"></param>
        /// <param name="plexTvShowSeasonKey">The rating key from the <see cref="PlexTvShowSeason"/></param>
        /// <param name="start">The start count.</param>
        /// <param name="retCount">The return count, how many items you want returned.</param>
        /// <returns></returns>
        public Task<PlexMediaContainer> GetAllEpisodesAsync(string authToken, string plexFullHost, int plexTvShowSeasonKey, int start = 0,
            int retCount = 100)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{plexTvShowSeasonKey}/children"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, start, retCount);
            request.AddQueryParameter("type", "4");
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        /// <summary>
        /// Returns all the Plex users for this account
        /// NOTE: For HOME USERS. There is no username or email, the user's home name is under the title property
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public Task<List<Friend>> GetUsersAsync(string authToken)
        {
            var request = new RestRequest(new Uri(FriendsUri), Method.GET, DataFormat.Xml);
            request = AddToken(request, authToken);
            return Client.SendRequestAsync<List<Friend>>(request);
        }

        public Task<PlexMediaContainer> GetRecentlyAddedAsync(string authToken, string hostUrl, string sectionId)
        {
            var request = new RestRequest(new Uri($"{hostUrl}/library/sections/{sectionId}/recentlyAdded"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, 0, 50);
            return Client.SendRequestAsync<PlexMediaContainer>(request);
        }

        public Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken)
        {
            var request = new RestRequest(new Uri(thumbUrl + "?width=150&height=226&minSize=1&upscale=1"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, 0, 50);
            return Client.SendImageRequestAsync(request);
        }

        /// <summary>
        /// Adds the required headers and also the authorization header.
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