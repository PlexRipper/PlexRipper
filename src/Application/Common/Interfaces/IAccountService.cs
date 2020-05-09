using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Task<Account> GetAccountAsync(int accountId);
        Task<List<Account>> GetAllAccountsAsync();
        Task<bool> ValidateAccountAsync(Account account);
        Task<bool> ValidateAccountAsync(string username, string password);

        /// <summary>
        /// Adds a new <see cref="Account"/> to the Database.
        /// </summary>
        /// <param name="username">The username of the <see cref="Account"/></param>
        /// <param name="password">The password of the <see cref="Account"/></param>
        /// <returns>The newly created <see cref="Account"/></returns>
        Task<Account> AddAccountAsync(string username, string password);

        Task<bool> DeleteAccountAsync(int accountId);
        Task<Account> GetAccountAsync(string username);
    }
}
