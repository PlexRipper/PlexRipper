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

        Task<bool> RemoveAccountAsync(int accountId);
        Task<bool> CheckIfUsernameIsAvailableAsync(string username);
        Task<List<PlexServer>> GetServersAsync(int accountId, bool refresh = false);
        Task<Account> CreateAccountAsync(Account newAccount);
        Task<Account> UpdateAccountAsync(Account newAccount);
    }
}
