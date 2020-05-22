using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPlexRipperDbContext _context;
        private readonly IPlexAccountRepository _plexAccountRepository;
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexServerRepository _plexServerRepository;
        private readonly IMapper _mapper;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private Serilog.ILogger Log { get; }

        #endregion Private Fields

        #region Public Constructors

        public PlexService(
            IPlexRipperDbContext context,
            IPlexAccountRepository plexAccountRepository,
            IPlexServerService plexServerService,
            IPlexServerRepository plexServerRepository,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi,
            IMapper mapper,
            Serilog.ILogger log)
        {
            _plexServiceApi = plexServiceApi;
            _context = context;
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
        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        public PlexAccount ConvertToPlexAccount(Account account)
        {
            if (account == null)
            {
                Log.Warning("The account was null");
                return null;
            }

            if (!account.IsValidated)
            {
                Log.Warning(
                    $"The account with Id: {account.Id} has not yet been confirmed." +
                           $" Confirm first before using ConvertToPlexAccount()");
                return null;
            }

            account = _context
                   .Accounts
                   .Include(x => x.PlexAccount)
                   .ThenInclude(x => x.PlexAccountServers)
                   .FirstOrDefault(x => x.Id == account.Id);
            return account?.PlexAccount;
        }

        public async Task<string> GetPlexToken(PlexAccount plexAccount)
        {
            return await _plexAuthenticationService.GetPlexToken(plexAccount);
        }



        public async Task<List<PlexServer>> GetServersAsync(PlexAccount plexAccount, bool refresh = false)
        {
            if (refresh)
            {
                await _plexServerService.RefreshServersAsync(plexAccount);
            }
            var x = await _plexServerService.GetServers(plexAccount);
            return x.ToList();
        }

        public async Task<PlexLibrary> GetLibrary(PlexServer plexServer)
        {
            var plexLibraries = await GetLibrariesAsync(plexServer);
            return Enumerable.First<PlexLibrary>(plexLibraries);
            //var result = await _plexApi.GetLibrary(plexServer.AccessToken, plexServer.BaseUrl, library.Key);
            //var metaData = await _plexApi.GetMetadata(plexServer.AccessToken, plexServer.BaseUrl, 5516);
            //string downloadUrl = _plexApi.GetDownloadUrl(plexServer, metaData);
            //string filename = _plexApi.GetDownloadFilename(plexServer, metaData);
            //_plexApi.DownloadMedia(plexServer.AccessToken, downloadUrl, filename);
            //return plexContainer;
        }

        /// <summary>
        /// Returns the list of all libraries belonging to this <see cref="PlexServer"/>
        /// </summary>
        /// <param name="plexServer">The PlexServer to retrieve the libraries</param>
        /// <param name="refresh">Force an Plex API update with the latest data</param>
        /// <returns>List of <see cref="PlexLibrary"/></returns>
        public async Task<List<PlexLibrary>> GetLibrariesAsync(PlexServer plexServer, bool refresh = false)
        {
            if (plexServer == null)
            {
                Log.Warning($"The {nameof(plexServer)} was null");
                return new List<PlexLibrary>();
            }

            var plexServerDB = await _context.PlexServers.FindAsync(plexServer.Id);

            if (plexServerDB != null && (plexServerDB.PlexLibraries == null || plexServerDB.PlexLibraries.Count == 0) || refresh)
            {
                var newLibraries = await _plexServiceApi.GetLibrarySections(plexServer.AccessToken, plexServer.BaseUrl);

                var libraries = await _context.PlexLibraries.Where(x => x.PlexServer.Id == plexServer.Id).ToListAsync();

                var librariesNotToRemove = new List<int>();

                // Update or add new plex libraries
                foreach (var newLibrary in newLibraries)
                {
                    var libraryDB = libraries.Find(x => x.Key == newLibrary.Key);

                    if (libraryDB != null)
                    {
                        // Update
                        libraryDB.Count = newLibrary.Count;
                        libraryDB.Key = newLibrary.Key;
                        libraryDB.Title = newLibrary.Title;
                        libraryDB.HasAccess = true;
                        libraryDB.PlexServer = plexServerDB;
                        librariesNotToRemove.Add(libraryDB.Id);
                    }
                    else
                    {
                        // Add
                        libraryDB = _mapper.Map<PlexLibrary>(newLibrary);
                        libraryDB.HasAccess = true;
                        libraryDB.PlexServer = plexServerDB;
                        await _context.PlexLibraries.AddAsync(libraryDB);
                    }
                }

                // Soft-delete any library that were either not updated or added
                foreach (var plexLibrary in libraries)
                {
                    if (!librariesNotToRemove.Contains(plexLibrary.Id))
                    {
                        var x = await _context.PlexLibraries.FindAsync(plexLibrary.Id);
                        x.HasAccess = false;
                    }
                }

                await _context.SaveChangesAsync();

            }

            return await _context.PlexLibraries.Where(x => x.PlexServer.Id == plexServer.Id).ToListAsync();
        }

        public async Task<List<PlexLibrary>> GetLibrariesByPlexServerIdAsync(int plexServerId, bool refresh = false)
        {
            if (plexServerId <= 0)
            {
                Log.Warning($"{nameof(plexServerId)} was 0 or lower and thus invalid!");
                return new List<PlexLibrary>();
            }

            var plexServer = await _context.PlexServers.FindAsync(plexServerId);

            if (plexServer == null)
            {
                Log.Warning($"The {nameof(plexServer)} was null");
                return new List<PlexLibrary>();
            }

            return await GetLibrariesAsync(plexServer);
        }

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
        public async Task<PlexAccount> GetPlexAccount(int plexAccountId)
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

        public async Task<PlexAccount> CreatePlexAccount(Account account, PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(plexAccount)} given as a parameter in {nameof(CreatePlexAccount)} was null.");
                return null;
            }

            plexAccount.ConfirmedAt = DateTime.Now;
            plexAccount.AccountId = account.Id;

            await _plexAccountRepository.AddAsync(plexAccount);
            return await _plexAccountRepository.GetAsync(plexAccount.Id);
        }

        public async Task<PlexAccount> UpdatePlexAccount(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(plexAccount)} given as a parameter in {nameof(UpdatePlexAccount)} was null.");
                return null;
            }
            plexAccount.ConfirmedAt = DateTime.Now;
            await _plexAccountRepository.UpdateAsync(plexAccount);
            return await _plexAccountRepository.GetAsync(plexAccount.Id);

        }

        #endregion
    }
}
