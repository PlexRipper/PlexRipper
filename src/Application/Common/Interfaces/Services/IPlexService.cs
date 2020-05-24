using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<bool> IsPlexAccountValid(string username, string password);
        Task<List<PlexServer>> GetServersAsync(PlexAccount plexAccount, bool refresh = false);
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);
        Task<PlexAccount> GetPlexAccount(long plexId);

        // Task<PlexLibrary> GetLibrary(PlexServer plexServer);
        Task<PlexAccount> RequestPlexAccountAsync(string username, string password);
        // Task<List<PlexLibrary>> GetLibrariesByPlexServerIdAsync(int plexServerId, bool refresh = false);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> based on the Id set by PlexRipper.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        Task<PlexAccount> GetPlexAccountAsync(int plexAccountId);

        Task<PlexAccount> CreatePlexAccountAsync(Account account, PlexAccount plexAccount);
        Task<PlexAccount> UpdatePlexAccountAsync(PlexAccount plexAccount);
    }
}
