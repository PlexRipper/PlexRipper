using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
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

        public async Task<Account> GetAccountAsync(string username, string password)
        {

            var result = await _context.Accounts.Include(x => x.PlexAccount)
                .FirstOrDefaultAsync(x => x.Username == username && x.Password == password);

            if (result != null)
            {
                return result;
            }

            _logger.LogWarning($"Could not find an Account with username: {username} and password: {password}");
            return null;
        }

        public async Task<Account> GetAccountAsync(int accountId)
        {

            var result = await _context.Accounts.FindAsync(accountId);

            if (result != null)
            {
                return result;
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

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        /// <summary>
        /// Adds a new <see cref="Account"/> to the Database.
        /// </summary>
        /// <param name="username">The username of the <see cref="Account"/></param>
        /// <param name="password">The password of the <see cref="Account"/></param>
        /// <returns>The newly created <see cref="Account"/></returns>
        public async Task<Account> AddAccountAsync(string username, string password)
        {
            try
            {
                var result = await GetAccountAsync(username, password);
                if (result != null)
                {
                    _logger.LogWarning("Account already exists in DB with these credentials");
                    return result;
                }

                _logger.LogDebug("Creating a new Account in DB");
                var account = new Account
                {
                    Username = username,
                    Password = password
                };

                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return account;

            }
            catch (Exception e)
            {
                _logger.LogError("Error while adding a new Account", e);
                throw;
            }
        }
        /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>Are the account credentials valid</returns>
        public async Task<bool> ValidateAccountAsync(Account account)
        {
            // Retrieve Account from DB
            var accountDB = _context.Accounts
                .Include(x => x.PlexAccount)
                .FirstOrDefault(x => x.Id == account.Id);

            if (accountDB == null)
            {
                _logger.LogWarning($"Could not find Account with id: {account.Id}", account);
                return false;
            }

            // Check if account is valid
            var plexAccount = await _plexService.IsAccountValid(accountDB);
            if (plexAccount != null)
            {
                // Account is valid
                _logger.LogDebug("Account credentials were valid");
                accountDB.IsConfirmed = true;
                accountDB.ConfirmedAt = DateTime.Now;
                accountDB.PlexAccount = await _context.PlexAccounts.FindAsync(plexAccount.Id);
            }
            else
            {
                // Account is invalid
                _logger.LogWarning("Account credentials were invalid");
                accountDB.IsConfirmed = false;
                accountDB.ConfirmedAt = DateTime.MinValue;
                _context.PlexAccounts.Remove(accountDB.PlexAccount);
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
