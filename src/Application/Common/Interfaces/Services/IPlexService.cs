using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<bool> IsPlexAccountValid(string username, string password);
        Task<List<PlexServer>> GetServers(PlexAccount plexAccount, bool refresh = false);
        Task<string> GetPlexToken(PlexAccount plexAccount);
        PlexAccount GetPlexAccount(long plexAccountId);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        PlexAccount ConvertToPlexAccount(Account account);

        Task<PlexLibrary> GetLibrary(PlexServer plexServer);
        Task<PlexAccount> RequestPlexAccountAsync(string username, string password);
        Task<PlexAccount> AddOrUpdatePlexAccount(Account account, PlexAccount plexAccountDto);
        Task<List<PlexLibrary>> GetLibrariesAsync(PlexServer plexServer, bool refresh = false);
        Task<List<PlexLibrary>> GetLibrariesByPlexServerIdAsync(int plexServerId, bool refresh = false);
    }
}
