using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;
using PlexRipper.PlexApi.Models.Server;
using RestSharp;
using DataFormat = RestSharp.DataFormat;

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
        /// This is for authenticating users credentials with Plex.
        /// <para>NOTE: Plex "Managed" users do not work.</para>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<PlexAccountDTO> PlexSignInAsync(string username, string password)
        {
            var userModel = new PlexUserRequest
            {
                User = new UserRequest
                {
                    Login = username,
                    Password = password,
                },
            };
            var request = new RestRequest(new Uri(SignInUri), Method.POST);
            request.AddJsonBody(userModel);

            var result = await Client.SendRequestAsync<PlexAccountDTO>(request);
            return result.ValueOrDefault;
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
            var request = new RestRequest(new Uri($"{serverBaseUrl}/identity"), Method.GET);
            request = AddToken(request, authToken);
            var response = await Client.SendRequestAsync<RestResponse>(request);
            if (response.IsFailed)
            {
                var error = response.Errors.First();
                if (error != null)
                {
                    return new PlexServerStatus
                    {
                        StatusCode = Convert.ToInt32(error.Metadata["StatusCode"]?.ToString() ?? "-1"),
                        StatusMessage = error.Metadata["Message"]?.ToString() ?? "Message not found",
                        LastChecked = DateTime.Now.ToUniversalTime(),
                        IsSuccessful = false,
                    };
                }
            }

            var reason = response.Reasons.First();
            var metaData = reason.Metadata;

            return new PlexServerStatus
            {
                StatusCode = metaData.ContainsKey("StatusCode") ? (int)metaData["StatusCode"] : -1,
                IsSuccessful = response.IsSuccess,
                StatusMessage = response.IsSuccess ? reason.Message :
                    metaData.ContainsKey("ErrorMessage") ? metaData["ErrorMessage"].ToString() : "ErrorMessageNotFound",
                LastChecked = DateTime.Now.ToUniversalTime(),
            };
        }

        public async Task<PlexAccountDTO> GetAccountAsync(string authToken)
        {
            var request = new RestRequest(new Uri(GetAccountUri), Method.GET);
            request = AddToken(request, authToken);
            var result = await Client.SendRequestAsync<PlexAccountDTO>(request);
            return result.ValueOrDefault;
        }

        public async Task<List<Server>> GetServerAsync(string authToken)
        {
            var request = new RestRequest(new Uri(ServerUri), Method.GET, DataFormat.Xml);
            request = AddToken(request, authToken);

            var result = await Client.SendRequestAsync<ServerContainer>(request);
            return result.ValueOrDefault?.Servers;
        }

        /// <summary>
        /// Returns an detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
        /// </summary>
        /// <param name="plexAuthToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        public async Task<PlexMediaContainer> GetLibrarySectionsAsync(string plexAuthToken, string plexFullHost)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections"), Method.GET);
            request = AddToken(request, plexAuthToken);
            Log.Debug($"GetLibrarySectionsAsync => {request.Resource}");
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataForLibraryAsync(string authToken, string plexServerBaseUrl, string libraryId)
        {
            var request = new RestRequest(new Uri($"{plexServerBaseUrl}/library/sections/{libraryId}/all"), Method.GET);
            request = AddToken(request, authToken);
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataAsync(string authToken, string plexFullHost, int metadataId)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{metadataId}"), Method.GET);
            request = AddToken(request, authToken);
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataAsync(string authToken, string metaDataUrl)
        {
            var request = new RestRequest(new Uri(metaDataUrl), Method.GET);
            request = AddToken(request, authToken);
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"), Method.GET);
            request = AddToken(request, authToken);
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        /// <summary>
        /// Gets all episodes.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="host">The host.</param>
        /// <param name="plexFullHost"></param>
        /// <param name="plexTvShowSeasonKey">The rating key from the <see cref="PlexTvShowSeason"/>.</param>
        /// <param name="start">The start count.</param>
        /// <param name="retCount">The return count, how many items you want returned.</param>
        /// <returns></returns>
        public async Task<PlexMediaContainer> GetAllEpisodesAsync(string authToken, string plexFullHost, int plexTvShowSeasonKey, int start = 0,
            int retCount = 5000)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{plexTvShowSeasonKey}/children"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, start, retCount);
            request.AddQueryParameter("type", "4");
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetRecentlyAddedAsync(string authToken, string hostUrl, string sectionId)
        {
            var request = new RestRequest(new Uri($"{hostUrl}/library/sections/{sectionId}/recentlyAdded"), Method.GET);
            request = AddToken(request, authToken);
            request = AddLimitHeaders(request, 0, 50);
            var result = await Client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken, int width = 0, int height = 0)
        {
            if (width > 0 && height > 0)
            {
                var uri = new Uri(thumbUrl);
                thumbUrl =
                    $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&width={width}&height={height}&minSize=1&upscale=1";
            }

            var request = new RestRequest(new Uri(thumbUrl), Method.GET);

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
            request.AddParameter("X-Plex-Token", authToken, ParameterType.QueryString);
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