using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IPlexRipperDbContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IPlexService _plexService;
        private ILogger Log { get; }

        public AccountService(IPlexRipperDbContext context, IAccountRepository accountRepository, IMapper mapper, IPlexService plexService, ILogger logger)
        {
            _context = context;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _plexService = plexService;
            Log = logger;
        }
        public async Task<List<PlexServer>> GetServers(int accountId, bool refresh = false)
        {
            var account = await GetAccountAsync(accountId);
            var plexAccount = _plexService.ConvertToPlexAccount(account);
            return await _plexService.GetServers(plexAccount, refresh);
        }
        /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Are the account credentials valid</returns>
        public async Task<bool> ValidateAccountAsync(string username, string password)
        {
            return await _plexService.IsPlexAccountValid(username, password);
        }
        /// /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>Are the account credentials valid</returns>
        public async Task<bool> ValidateAccountAsync(Account account)
        {
            return await _plexService.IsPlexAccountValid(account.Username, account.Password);
        }

        #region CRUD
        public async Task<Account> GetAccountAsync(string username)
        {
            var result = await _accountRepository
                .FindWithIncludeAsync(x => x.Username == username);

            if (result != null)
            {
                result.PlexAccount.Account = null; // TODO Might be removed
                return result;
            }

            Log.Warning($"Could not find an Account with username: {username}");
            return null;
        }

        /// <summary>
        /// Returns the Account as noTracking
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<Account> GetAccountAsync(int accountId)
        {
            var result = await _accountRepository.GetWithIncludeAsync(accountId);

            if (result != null)
            {
                return await GetAccountAsync(result.Username);
            }

            Log.Warning($"Could not find an Account with id: {accountId}");
            return null;
        }

        public async Task<List<Account>> GetAllAccountsAsync(bool onlyEnabled = false)
        {
            if (onlyEnabled)
            {
                Log.Debug("Returning only enabled account");
                var result = await _accountRepository
                    .FindAllWithIncludeAsync(x => x.IsEnabled);
                return result.ToList();
            }
            else
            {
                Log.Debug("Returning all accounts");
                var result = await _accountRepository.GetAllWithIncludeAsync();
                return result.ToList();
            }
        }


        public async Task<Account> CreateAccountAsync(Account newAccount)
        {
            var result = await _accountRepository.FindAsync(
                x => x.Username == newAccount.Username &&
                     x.Password == newAccount.Password);
            if (result == null)
            {
                Log.Information("Creating a new Account in DB");
                await _accountRepository.AddAsync(newAccount);

                var accountDB = await _accountRepository.GetAsync(newAccount.Id);

                await SetupAccountAsync(accountDB);

                return accountDB;
            }

            Log.Warning("An account with these credentials already exists!");
            return null;
        }

        public async Task<Account> UpdateAccountAsync(Account newAccount)
        {
            if (newAccount == null)
            {
                Log.Warning("The account was null");
                return null;
            }
            if (newAccount.Id <= 0)
            {
                Log.Warning("The Id was 0 or lower");
                return null;
            }

            await _accountRepository.UpdateAsync(newAccount);

            var accountDB = await _accountRepository.GetAsync(newAccount.Id);

            // Re-validate if the password changed
            if (accountDB.Password != newAccount.Password)
            {
                await SetupAccountAsync(accountDB);
            }

            return accountDB;
        }


        /// <summary>
        /// Adds a new <see cref="Account"/> to the Database.
        /// </summary>
        /// <param name="newAccount"></param>
        /// <returns>The newly created <see cref="Account"/></returns>
        public async Task<Account> AddOrUpdateAccountAsync(Account newAccount)
        {
            try
            {
                bool isNew = false;
                bool isUpdated = false;
                var accountDB = await _context.Accounts.FirstOrDefaultAsync(x => x.Username == newAccount.Username);

                // Update other values
                accountDB.DisplayName = newAccount.DisplayName;
                accountDB.IsEnabled = newAccount.IsEnabled;

                // Request and setup PlexAccount from API and add to Account
                if (isNew || isUpdated)
                {

                }

                await _context.SaveChangesAsync();
                return accountDB;

            }
            catch (Exception e)
            {
                Log.Error("Error while adding or updating a new Account", e);
                throw;
            }
        }

        public async Task<bool> SetupAccountAsync(Account account)
        {
            if (account == null)
            {
                Log.Warning($"{nameof(SetupAccountAsync)} => The account was null");
                return false;
            }

            account = await _accountRepository.GetAsync(account.Id);

            // Request new PlexAccount
            var plexAccount = await _plexService.RequestPlexAccountAsync(account.Username, account.Password);


            if (plexAccount == null)
            {
                Log.Warning($"{nameof(SetupAccountAsync)} => The plexAccount was null");
                return false;
            }

            if (account.PlexAccount == null)
            {
                // Create
                await _plexService.CreatePlexAccount(account, plexAccount);
            }
            else
            {
                // Update
                await _plexService.UpdatePlexAccount(plexAccount);
            }

            account.PlexAccount = plexAccount;
            account.IsValidated = true;
            account.ValidatedAt = DateTime.Now;
            await UpdateAccountAsync(account);
            return true;

            // TODO Refresh the Plex Servers
            // await GetServers(accountDB.Id, true);
        }


        public async Task<bool> RemoveAccountAsync(int accountId)
        {
            return await _accountRepository.RemoveAsync(accountId);
        }
        #endregion
    }
}
