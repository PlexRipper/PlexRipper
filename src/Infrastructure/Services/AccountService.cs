using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IPlexRipperDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPlexService _plexService;

        public AccountService(IPlexRipperDbContext context, IMapper mapper, IPlexService plexService)
        {
            _context = context;
            _mapper = mapper;
            _plexService = plexService;
        }

        public Account AddAccount(string username, string password)
        {

            var account = new Account()
            {
                Username = username,
                Password = password
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return account;
        }

        public async Task<bool> ValidateAccount(Account account)
        {
            var accountDB = _context.Accounts.Find(account.Id);
            if (accountDB == null)
            {
                // TODO Add error logging here
                return false;
            }

            var plexAccount = await _plexService.IsAccountValid(accountDB);
            if (plexAccount != null)
            {
                accountDB.IsConfirmed = true;
                accountDB.ConfirmedAt = DateTime.Now;
                accountDB.PlexAccount = _context.PlexAccounts.Find(plexAccount.Id);
            }
            else
            {
                accountDB.IsConfirmed = false;
                accountDB.ConfirmedAt = DateTime.MinValue;
                _context.PlexAccounts.Remove(accountDB.PlexAccount);
            }
            await _context.SaveChangesAsync();
            return false;
        }
    }
}
