using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Task<Account> GetAccountAsync(int accountId);
        Task<List<Account>> GetAllAccountsAsync(bool onlyEnabled = false);
        Task<bool> ValidateAccountAsync(Account account);
        Task<bool> ValidateAccountAsync(string username, string password);

        /// <summary>
        /// Adds a new <see cref="Account"/> to the Database.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>The newly created <see cref="Account"/></returns>
        Task<Account> AddOrUpdateAccountAsync(Account account);

        Task<bool> DeleteAccountAsync(int accountId);
        Task<Account> GetAccountAsync(string username);
        Task<List<PlexServer>> GetServers(int accountId, bool refresh = false);
    }
}
