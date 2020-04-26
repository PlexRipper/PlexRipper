using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Account AddAccount(string username, string password);
        Task<bool> ValidateAccount(Account account);
    }
}
