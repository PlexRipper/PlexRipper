using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IPlexRipperDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPlexService _plexService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IPlexRipperDbContext context, IMapper mapper, IPlexService plexService, ILogger<AccountService> logger)
        {
            _context = context;
            _mapper = mapper;
            _plexService = plexService;
            _logger = logger;
        }

        public async Task<Account> GetAccountAsync(string username)
        {

            var result = await _context.Accounts
                .Include(x => x.PlexAccount)
                .ThenInclude(x => x.PlexAccountServers)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username);

            if (result != null)
            {
                result.PlexAccount.Account = null;
                return result;
            }

            _logger.LogWarning($"Could not find an Account with username: {username}");
            return null;
        }

        /// <summary>
        /// Returns the Account as noTracking
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<Account> GetAccountAsync(int accountId)
        {

            var result = await _context.Accounts.AsNoTracking().Where(x => x.Id == accountId).FirstOrDefaultAsync();
            if (result != null)
            {
                return await GetAccountAsync(result.Username);
            }

            _logger.LogWarning($"Could not find an Account with id: {accountId}");
            return null;
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var result = await GetAccountAsync(accountId);
            if (result != null)
            {
                _context.Accounts.Remove(result);

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Account>> GetAllAccountsAsync(bool onlyEnabled = false)
        {
            List<Account> accounts = await _context.Accounts
                .Include(x => x.PlexAccount)
                .ThenInclude(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexServer)
                .AsNoTracking()
                .ToListAsync();

            return onlyEnabled ? accounts.Where(x => x.IsEnabled).ToList() : accounts;
        }
        public async Task<List<PlexServer>> GetServers(int accountId, bool refresh = false)
        {
            var account = await GetAccountAsync(accountId);
            var plexAccount = _plexService.ConvertToPlexAccount(account);
            return await _plexService.GetServers(plexAccount, refresh);
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

                // Add new
                if (accountDB == null)
                {
                    _logger.LogInformation("Creating a new Account in DB");

                    await _context.Accounts.AddAsync(newAccount);
                    await _context.SaveChangesAsync();
                    accountDB = await _context.Accounts.FirstOrDefaultAsync(x => x.Username == newAccount.Username);
                    isNew = true;
                }

                // Re-validate if the password changed
                if (accountDB.Password != newAccount.Password)
                {
                    accountDB.Password = newAccount.Password;
                    isUpdated = true;
                }

                // Update other values
                accountDB.DisplayName = newAccount.DisplayName;
                accountDB.IsEnabled = newAccount.IsEnabled;

                // Request and setup PlexAccount from API and add to Account
                if (isNew || isUpdated)
                {
                    var plexAccountDTO = await _plexService.RequestPlexAccountAsync(accountDB.Username, accountDB.Password);
                    if (plexAccountDTO != null)
                    {
                        var plexAccount = await _plexService.AddOrUpdatePlexAccount(accountDB, plexAccountDTO);
                        if (plexAccount != null)
                        {
                            accountDB.PlexAccount = plexAccount;
                            accountDB.IsValidated = true;
                            accountDB.ValidatedAt = DateTime.Now;
                        }

                        // Refresh the Plex Servers
                        await GetServers(accountDB.Id, true);
                    }
                }

                await _context.SaveChangesAsync();
                return accountDB;

            }
            catch (Exception e)
            {
                _logger.LogError("Error while adding or updating a new Account", e);
                throw;
            }
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
    }
}
