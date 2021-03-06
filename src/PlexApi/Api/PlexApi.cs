﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Helpers;
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
            _client = client;
        }

        private PlexApiClient _client { get; }

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

            var result = await _client.SendRequestAsync<PlexAccountDTO>(request);
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
            request.AddToken(authToken);
            var response = await _client.SendRequestAsync<RestResponse>(request);
            if (response.IsFailed)
            {
                var error = response.Errors.First();
                if (error != null)
                {
                    return new PlexServerStatus
                    {
                        StatusCode = Convert.ToInt32(error.Metadata["StatusCode"]?.ToString() ?? "-1"),
                        StatusMessage = error.Metadata["ErrorMessage"]?.ToString() ?? "Message not found",
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
            request.AddToken(authToken);
            var result = await _client.SendRequestAsync<PlexAccountDTO>(request);
            return result.ValueOrDefault;
        }

        public async Task<List<Server>> GetServerAsync(string authToken)
        {
            var request = new RestRequest(new Uri(ServerUri), Method.GET, DataFormat.Xml);
            request.AddToken(authToken);

            var result = await _client.SendRequestAsync<ServerContainer>(request);
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
            request.AddToken(plexAuthToken);
            Log.Debug($"GetLibrarySectionsAsync => {request.Resource}");
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataForLibraryAsync(string authToken, string plexServerBaseUrl, string libraryKey)
        {
            var request = new RestRequest(new Uri($"{plexServerBaseUrl}/library/sections/{libraryKey}/all"), Method.GET);
            request.AddToken(authToken);
            request.AddQueryParameter("includeMeta", "1");
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataAsync(string authToken, string plexFullHost, int metadataId)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{metadataId}"), Method.GET);
            request.AddToken(authToken);
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetMetadataAsync(string authToken, string metaDataUrl)
        {
            var request = new RestRequest(new Uri(metaDataUrl), Method.GET);
            request.AddToken(authToken);
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"), Method.GET);
            request.AddToken(authToken);
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        /// <summary>
        /// Gets all seasons contained within a media container.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="plexFullHost"></param>
        /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary"/>.</param>
        /// <returns></returns>
        public async Task<PlexMediaContainer> GetAllSeasonsAsync(string authToken, string plexFullHost, string plexLibraryKey)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections/{plexLibraryKey}/all"), Method.GET);
            request.AddToken(authToken);
            request.AddQueryParameter("type", "3");
            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        /// <summary>
        /// Gets all episodes within a media container.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="plexFullHost"></param>
        /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary"/>.</param>
        /// <returns></returns>
        public async Task<PlexMediaContainer> GetAllEpisodesAsync(string authToken, string plexFullHost, string plexLibraryKey, int from, int to)
        {
            var request = new RestRequest(new Uri($"{plexFullHost}/library/sections/{plexLibraryKey}/all"), Method.GET);
            request.AddToken(authToken).AddLimitHeaders(from, to);
            request.AddQueryParameter("type", "4");

            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<PlexMediaContainer> GetRecentlyAddedAsync(string authToken, string hostUrl, string sectionId)
        {
            var request = new RestRequest(new Uri($"{hostUrl}/library/sections/{sectionId}/recentlyAdded"), Method.GET);
            request.AddToken(authToken).AddLimitHeaders(0, 50);

            var result = await _client.SendRequestAsync<PlexMediaContainer>(request);
            return result.ValueOrDefault;
        }

        public async Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken, int width = 0, int height = 0)
        {
            if (width > 0 && height > 0)
            {
                var uri = new Uri(thumbUrl);
                thumbUrl =
                    $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&width={width}&height={height}&minSize=1&upscale=1";
            }

            var request = new RestRequest(new Uri(thumbUrl), Method.GET);

            request.AddToken(authToken).AddLimitHeaders(0, 50);

            var result = await _client.SendImageRequestAsync(request);
            return result;
        }




    }
}