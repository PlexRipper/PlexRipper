using PlexRipper.Application.Common.DTO.Plex;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Domain.Common.API;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.DTO.PlexGetLibrarySections;
using PlexRipper.Infrastructure.Common.DTO.PlexGetServer;
using PlexRipper.Infrastructure.Common.DTO.PlexGetStatus;
using PlexRipper.Infrastructure.Common.DTO.PlexLibrary;
using PlexRipper.Infrastructure.Common.DTO.PlexLibraryMedia;
using PlexRipper.Infrastructure.Common.DTO.PlexSignIn;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Common.Models.OAuth;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.API.Plex
{
    public class PlexApi : IPlexApi
    {
        public PlexApi(IApi api, Serilog.ILogger logger)
        {
            Log = logger;
            Api = api;
        }

        private IApi Api { get; }
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
                User = new UserRequestDTO()
                {
                    Login = username,
                    Password = password
                }
            };
            var request = new Request(SignInUri, string.Empty, HttpMethod.Post);

            AddHeaders(request);
            request.AddJsonBody(userModel);

            return Api.Request<PlexAuthenticationDTO>(request);
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

        public Task<PlexStatusDTO> GetStatus(string authToken, string uri)
        {
            var request = new Request(uri, string.Empty, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexStatusDTO>(request);
        }

        public Task<PlexAccount> GetAccount(string authToken)
        {
            var request = new Request(GetAccountUri, string.Empty, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexAccount>(request);
        }

        public Task<PlexServerContainerXML> GetServer(string authToken)
        {
            var request = new Request(ServerUri, string.Empty, HttpMethod.Get, ContentType.Xml);

            AddHeaders(request, authToken);

            return Api.Request<PlexServerContainerXML>(request);
        }

        /// <summary>
        /// Returns an detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
        /// </summary>
        /// <param name="plexAuthToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        public Task<PlexLibrarySectionsDTO> GetLibrarySections(string plexAuthToken, string plexFullHost)
        {
            var request = new Request("library/sections", plexFullHost, HttpMethod.Get);
            AddHeaders(request, plexAuthToken);
            return Api.Request<PlexLibrarySectionsDTO>(request);
        }

        public Task<PlexLibraryMediaDTO> GetLibraryMediaAsync(string authToken, string plexFullHost, string libraryId)
        {
            var request = new Request($"library/sections/{libraryId}/all", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexLibraryMediaDTO>(request);
        }

        public Task<PlexLibrariesForMachineId> GetLibrariesForMachineId(string authToken, string machineId)
        {
            var request = new Request("", $"https://plex.tv/api/servers/{machineId}", HttpMethod.Get, ContentType.Xml);
            AddHeaders(request, authToken);
            return Api.Request<PlexLibrariesForMachineId>(request);
        }

        public bool DownloadMedia(string authToken, string downloadUrl, string fileName)
        {
            //var request = new Request($"library/metadata/{mediaKey}/?download=1", plexFullHost, HttpMethod.Get);
            //string url = $"{plexFullHost}library/metadata/{mediaKey}/?download=1&X-Plex-Token={authToken}";
            Log.Debug(downloadUrl);

            var request = new Request($"{downloadUrl}?X-Plex-Token={authToken}", HttpMethod.Get);
            AddHeaders(request, authToken);
            //Api.Download(request, fileName);

            return true;


            //WebClient webClient = new WebClient();
            //webClient.DownloadProgressChanged += ((sender, args) =>
            //{
            //    Log.Information(args.ProgressPercentage.ToString());
            //});
            //webClient.DownloadFileCompleted += ((sender, args) =>
            //{
            //    Log.Information("Download has completed!");
            //});

            //webClient.DownloadFileAsync(new Uri(downloadUrl), @$"D:\Downloads\PlexDownloads\{fileName}");
            // AddHeaders(request, authToken);

            //return await Api.Request<PlexLibraryDTO>(request);

        }

        /// <summary>
        // 192.168.1.69:32400/library/metadata/3662/allLeaves
        // The metadata ratingkey should be in the Cache
        // Search for it and then call the above with the Directory.RatingKey
        // THEN! We need the episode metadata using result.Vide.Key ("/library/metadata/3664")
        // We then have the GUID which contains the TVDB ID plus the season and episode number: guid="com.plexapp.agents.thetvdb://269586/2/8?lang=en"
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="plexFullHost"></param>
        /// <param name="ratingKey"></param>
        /// <returns></returns>
        public Task<PlexMetadata> GetEpisodeMetaData(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new Request($"/library/metadata/{ratingKey}", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexMetadata>(request);
        }

        public Task<PlexMediaMetaDataDTO> GetMetadata(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new Request($"library/metadata/{ratingKey}", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexMediaMetaDataDTO>(request);
        }

        public Task<PlexMediaMetaDataDTO> GetMetadata(string authToken, string metaDataUrl)
        {
            var request = new Request(metaDataUrl, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexMediaMetaDataDTO>(request);
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


        public Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new Request($"library/metadata/{ratingKey}/children", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return Api.Request<PlexMetadata>(request);
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
        public Task<PlexLibraryContainerDTO> GetAllEpisodes(string authToken, string host, string section, int start, int retCount)
        {
            var request = new Request($"/library/sections/{section}/all", host, HttpMethod.Get);

            request.AddQueryString("type", "4");
            AddLimitHeaders(request, start, retCount);
            AddHeaders(request, authToken);

            return Api.Request<PlexLibraryContainerDTO>(request);
        }

        /// <summary>
        /// Retuns all the Plex users for this account
        /// NOTE: For HOME USERS. There is no username or email, the user's home name is under the title property
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public Task<PlexFriendsXML> GetUsers(string authToken)
        {
            var request = new Request(string.Empty, FriendsUri, HttpMethod.Get, ContentType.Xml);
            AddHeaders(request, authToken);

            return Api.Request<PlexFriendsXML>(request);
        }

        public Task<PlexMetadata> GetRecentlyAdded(string authToken, string uri, string sectionId)
        {
            var request = new Request($"library/sections/{sectionId}/recentlyAdded", uri, HttpMethod.Get);
            AddHeaders(request, authToken);
            AddLimitHeaders(request, 0, 50);

            return Api.Request<PlexMetadata>(request);
        }

        public Task<OAuthPin> GetPin(int pinId)
        {
            var request = new Request($"api/v2/pins/{pinId}", "https://plex.tv/", HttpMethod.Get);
            AddHeaders(request);

            return Api.Request<OAuthPin>(request);
        }

        public Uri GetOAuthUrl(string code, string applicationUrl)
        {
            var request = new Request("auth#", "https://app.plex.tv", HttpMethod.Get);
            AddHeaders(request);

            request.AddQueryString("code", code);
            request.AddQueryString("context[device][product]", "Saverr");  // TODO Debate if we should put PlexRipper here
            request.AddQueryString("context[device][environment]", "bundled");
            request.AddQueryString("context[device][layout]", "desktop");
            request.AddQueryString("context[device][platform]", "Web");
            request.AddQueryString("context[device][device]", "Ombi");

            request.AddQueryString("clientID", "271938");

            if (request.FullUri.Fragment.Equals("#"))
            {
                var uri = request.FullUri.ToString();
                var withoutEnd = uri.Remove(uri.Length - 1, 1);
                var startOfQueryLocation = withoutEnd.IndexOf('?');
                var better = withoutEnd.Insert(startOfQueryLocation, "#");
                request.FullUri = new Uri(better);
            }

            return request.FullUri;
        }

        public async Task<PlexAddWrapper> AddUser(string emailAddress, string serverId, string authToken, int[] libs)
        {
            var request = new Request(string.Empty, $"https://plex.tv/api/servers/{serverId}/shared_servers", HttpMethod.Post, ContentType.Xml);
            AddHeaders(request, authToken);
            request.AddJsonBody(new
            {
                server_id = serverId,
                shared_server = new
                {
                    library_section_ids = libs.Length > 0 ? libs : new int[] { },
                    invited_email = emailAddress
                },
                sharing_settings = new { }
            });
            var result = await Api.RequestContent(request);
            try
            {
                var add = Api.DeserializeXml<PlexAddXML>(result);
                return new PlexAddWrapper { AddXml = add };
            }
            catch (InvalidOperationException)
            {
                var error = Api.DeserializeXml<AddUserError>(result);
                return new PlexAddWrapper { Error = error };
            }
        }


        /// <summary>
        /// Adds the required headers and also the authorization header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authToken"></param>
        private void AddHeaders(Request request, string authToken)
        {
            request.AddHeader("X-Plex-Token", authToken);
            AddHeaders(request);
        }

        /// <summary>
        /// Adds the main required headers to the Plex Request
        /// </summary>
        /// <param name="request"></param>
        private void AddHeaders(Request request)
        {
            request.AddHeader("X-Plex-Client-Identifier", "271938");
            request.AddHeader("X-Plex-Product", "Saverr"); // TODO Debate if we should put PlexRipper here
            request.AddHeader("X-Plex-Version", "3");
            request.AddHeader("X-Plex-Device", "Ombi");
            request.AddHeader("X-Plex-Platform", "Web");
            request.AddContentHeader("Content-Type", request.ContentType == ContentType.Json ? "application/json" : "application/xml");
            request.AddHeader("Accept", "application/json");
        }

        private void AddLimitHeaders(Request request, int from, int to)
        {
            request.AddHeader("X-Plex-Container-Start", from.ToString());
            request.AddHeader("X-Plex-Container-Size", to.ToString());
        }


    }
}
