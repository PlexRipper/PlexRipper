using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexApiService
    {
        Task<PlexAccount> PlexSignInAsync(string username, string password);
        Task<string> RefreshPlexAuthTokenAsync(Account account);
        Task<PlexAccount> GetAccountAsync(string authToken);
        Task<PlexLibrary> GetLibraryAsync(string authToken, string plexFullHost, string libraryId);
        Task<List<PlexServer>> GetServerAsync(string authToken);
        Task<List<PlexLibrary>> GetLibrarySections(string authToken, string plexFullHost);
    }
}
