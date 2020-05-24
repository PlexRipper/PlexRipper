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
        private readonly IAccountRepository _accountRepository;
        private readonly IPlexService _plexService;
        private readonly IPlexServerService _plexServerService;
        private ILogger Log { get; }

        public AccountService(IAccountRepository accountRepository, IPlexService plexService, IPlexServerService plexServerService, ILogger logger)
        {
            _accountRepository = accountRepository;
            _plexService = plexService;
            _plexServerService = plexServerService;
            Log = logger;
        }
        public async Task<List<PlexServer>> GetServersAsync(int accountId, bool refresh = false)
        {
            var account = await GetAccountAsync(accountId).ConfigureAwait(true);
            return await _plexService.GetServersAsync(account.PlexAccount, refresh).ConfigureAwait(true);
        }
        /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Are the account credentials valid</returns>
        public Task<bool> ValidateAccountAsync(string username, string password)
        {
            return _plexService.IsPlexAccountValid(username, password);
        }
        /// /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>Are the account credentials valid</returns>
        public Task<bool> ValidateAccountAsync(Account account)
        {
            return _plexService.IsPlexAccountValid(account.Username, account.Password);
        }

        public async Task<bool> SetupAccountAsync(Account account)
        {
            if (account == null)
            {
                Log.Warning($"{nameof(SetupAccountAsync)} => The account was null");
                return false;
            }
            Log.Debug("Setting up new Account");

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
                await _plexService.CreatePlexAccountAsync(account, plexAccount);
            }
            else
            {
                // Update
                await _plexService.UpdatePlexAccountAsync(plexAccount);
            }

            account.PlexAccount = plexAccount;
            account.IsValidated = true;
            account.ValidatedAt = DateTime.Now;
            await UpdateAccountAsync(account);

            // Retrieve and store servers
            await _plexServerService.RefreshPlexServersAsync(plexAccount);

            Log.Debug("Account was setup successfully!");
            return true;

            // TODO Refresh the Plex Servers
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


        public async Task<bool> RemoveAccountAsync(int accountId)
        {
            return await _accountRepository.RemoveAsync(accountId);
        }
        #endregion
    }
}
