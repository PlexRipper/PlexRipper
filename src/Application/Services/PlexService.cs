using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexService : IPlexService
    {

        #region Private Fields

        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string PlexServersUrl = "https://plex.tv/pms/servers.xml";
        private const string PlexSignInUrl = "https://plex.tv/users/sign_in.json";

        private static readonly HttpClient client = new HttpClient();
        private readonly IPlexApiService _plexServiceApi;
        private readonly IPlexAccountRepository _plexAccountRepository;
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexServerRepository _plexServerRepository;
        private readonly IMapper _mapper;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private Serilog.ILogger Log { get; }

        #endregion Private Fields

        #region Public Constructors

        public PlexService(
            IPlexAccountRepository plexAccountRepository,
            IPlexServerService plexServerService,
            IPlexServerRepository plexServerRepository,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi,
            IMapper mapper,
            Serilog.ILogger log)
        {
            _plexServiceApi = plexServiceApi;
            _plexAccountRepository = plexAccountRepository;
            _plexServerService = plexServerService;
            _plexServerRepository = plexServerRepository;
            _mapper = mapper;
            Log = log;
            client.Timeout = new TimeSpan(0, 0, 0, 30);
            _plexAuthenticationService = plexAuthenticationService;
        }

        #endregion Public Constructors

        #region Public Methods

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexToken(plexAccount);
        }

        public async Task<List<PlexServer>> GetServersAsync(PlexAccount plexAccount, bool refresh = false)
        {
            if (refresh)
            {
                await _plexServerService.RefreshPlexServersAsync(plexAccount);
            }
            var x = await _plexServerService.GetServers(plexAccount);
            return x.ToList();
        }

        //public async Task<PlexLibrary> GetLibrary(PlexServer plexServer)
        //{
        //    var plexLibraries = await GetLibrariesAsync(plexServer);
        //    return Enumerable.First<PlexLibrary>(plexLibraries);
        //    //var result = await _plexApi.GetLibrary(plexServer.AccessToken, plexServer.BaseUrl, library.Key);
        //    //var metaData = await _plexApi.GetMetadata(plexServer.AccessToken, plexServer.BaseUrl, 5516);
        //    //string downloadUrl = _plexApi.GetDownloadUrl(plexServer, metaData);
        //    //string filename = _plexApi.GetDownloadFilename(plexServer, metaData);
        //    //_plexApi.DownloadMedia(plexServer.AccessToken, downloadUrl, filename);
        //    //return plexContainer;
        //}

        /// <summary>
        /// Check the validity of Plex credentials to the Plex API. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>The result of the test</returns>
        public async Task<bool> IsPlexAccountValid(string username, string password)
        {
            return await RequestPlexAccountAsync(username, password) != null;
        }


        public async Task<PlexAccount> RequestPlexAccountAsync(string username, string password)
        {
            return await _plexServiceApi.PlexSignInAsync(username, password);
        }

        #endregion Public Methods

        #region CRUD

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> based on the Id set by PlexRipper.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        public async Task<PlexAccount> GetPlexAccountAsync(int plexAccountId)
        {
            return await _plexAccountRepository.GetAsync(plexAccountId);
        }

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> based on the PlexId set by the PlexAPI.
        /// </summary>
        /// <param name="plexId"></param>
        /// <returns></returns>
        public async Task<PlexAccount> GetPlexAccount(long plexId)
        {
            return await _plexAccountRepository.FindAsync(x => x.PlexId == plexId);
        }

        public async Task<PlexAccount> CreatePlexAccountAsync(Account account, PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(plexAccount)} given as a parameter in {nameof(CreatePlexAccountAsync)} was null.");
                return null;
            }

            plexAccount.ConfirmedAt = DateTime.Now;
            plexAccount.AccountId = account.Id;

            await _plexAccountRepository.AddAsync(plexAccount);
            return await _plexAccountRepository.GetAsync(plexAccount.Id);
        }

        public async Task<PlexAccount> UpdatePlexAccountAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(plexAccount)} given as a parameter in {nameof(UpdatePlexAccountAsync)} was null.");
                return null;
            }
            plexAccount.ConfirmedAt = DateTime.Now;
            await _plexAccountRepository.UpdateAsync(plexAccount);
            return await _plexAccountRepository.GetAsync(plexAccount.Id);

        }

        #endregion
    }
}
