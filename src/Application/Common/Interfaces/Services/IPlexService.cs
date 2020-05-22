using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<bool> IsPlexAccountValid(string username, string password);
        Task<List<PlexServer>> GetServersAsync(PlexAccount plexAccount, bool refresh = false);
        Task<string> GetPlexToken(PlexAccount plexAccount);
        Task<PlexAccount> GetPlexAccount(long plexId);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        PlexAccount ConvertToPlexAccount(Account account);

        Task<PlexLibrary> GetLibrary(PlexServer plexServer);
        Task<PlexAccount> RequestPlexAccountAsync(string username, string password);
        Task<List<PlexLibrary>> GetLibrariesAsync(PlexServer plexServer, bool refresh = false);
        Task<List<PlexLibrary>> GetLibrariesByPlexServerIdAsync(int plexServerId, bool refresh = false);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> based on the Id set by PlexRipper.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        Task<PlexAccount> GetPlexAccount(int plexAccountId);

        Task<PlexAccount> CreatePlexAccount(Account account, PlexAccount plexAccount);
        Task<PlexAccount> UpdatePlexAccount(PlexAccount plexAccount);
    }
}
