using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Common.Models.Plex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    public class PlexService : IPlexService
    {

        #region Private Fields

        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string PlexServersUrl = "https://plex.tv/pms/servers.xml";
        private const string PlexSignInUrl = "https://plex.tv/users/sign_in.json";
        private static readonly HttpClient client = new HttpClient();
        private readonly IPlexApi _plexApi;
        private readonly IPlexRipperDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlexService> _logger;

        #endregion Private Fields

        #region Public Constructors

        public PlexService(IPlexRipperDbContext context, IMapper mapper, IPlexApi plexApi, ILogger<PlexService> logger)
        {
            _plexApi = plexApi;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            client.Timeout = new TimeSpan(0, 0, 0, 30);

        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<PlexAccount> AddOrUpdatePlexAccount(Account account, PlexAccountDTO plexAccountDto)
        {
            if (plexAccountDto == null)
            {
                _logger.LogError($"PlexAccountDTO given as a parameter in {nameof(AddOrUpdatePlexAccount)} was null.");
                return null;
            }


            var accountDB = await _context.Accounts.FindAsync(account.Id);

            PlexAccount plexAccount = _mapper.Map<PlexAccount>(plexAccountDto);
            var result = await _context.PlexAccounts.FindAsync(plexAccount.Id);
            if (result != null)
            {
                _logger.LogDebug($"PlexAccount with Id: {result.Id} already exists, will update now");
                // Update
                result = plexAccount;
                plexAccount.Account = accountDB;
                result.ConfirmedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return result;

            }

            // Add
            _logger.LogDebug($"PlexAccount with Id: {plexAccount.Id} does not yet exist, will add now");
            plexAccount.ConfirmedAt = DateTime.Now;
            plexAccount.Account = accountDB;
            await _context.PlexAccounts.AddAsync(plexAccount);
            await _context.SaveChangesAsync();
            return plexAccount;
        }

        public PlexAccount GetPlexAccount(long plexAccountId)
        {
            return _context.PlexAccounts.FirstOrDefault(x => x.Id == plexAccountId);
        }

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        public PlexAccount ConvertToPlexAccount(Account account)
        {
            if (!account.IsConfirmed)
            {
                _logger.LogWarning(
                    $"The account with Id: {account.Id} has not yet been confirmed." +
                           $" Confirm first before using ConvertToPlexAccount()");
                return null;
            }

            account = _context
                   .Accounts
                   .Include(x => x.PlexAccount)
                   .FirstOrDefault(x => x.Id == account.Id);
            return account?.PlexAccount;
        }

        public async Task<string> GetPlexToken(Account account)
        {
            var plexAccount = ConvertToPlexAccount(account);

            if (plexAccount == null)
            {
                _logger.LogWarning($"plexAccount result, converted from account with Id: {account.Id}, was null");
                return string.Empty;
            }

            if (plexAccount.AuthToken != string.Empty)
            {
                // TODO Make the token refresh limit configurable 
                if ((plexAccount.ConfirmedAt - DateTime.Now).TotalDays < 30)
                {
                    _logger.LogInformation("Plex AuthToken was still valid, using from local DB.");
                    return plexAccount.AuthToken;
                }
                _logger.LogInformation("Plex AuthToken has expired, refreshing Plex AuthToken now.");
                return await _plexApi.RefreshPlexAuthTokenAsync(account);
            }

            _logger.LogError($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!");
            return string.Empty;
        }

        public async Task<List<string>> GetServers(Account account)
        {
            var token = await GetPlexToken(account);

            if (!string.IsNullOrEmpty(token))
            {
                var result = await _plexApi.GetServer(token);
                _logger.LogDebug("response: ", result);
            }

            return new List<string>();
        }

        /// <summary>
        /// Check the validity of <see cref="Account"/> credentials to the Plex API. 
        /// </summary>
        /// <param name="account">The Account to be validated</param>
        /// <returns>The PlexAccount in DB that is returned from the Plex API</returns>
        public async Task<PlexAccount> IsAccountValid(Account account)
        {
            var accountDB = await _context.Accounts.FindAsync(account.Id);
            var plexAuthentication = await _plexApi.PlexSignInAsync(account);
            if (plexAuthentication.User != null)
            {
                return await AddOrUpdatePlexAccount(accountDB, plexAuthentication.User);
            }
            // TODO Add error logging here
            return null;
        }
        #endregion Public Methods

    }
}
