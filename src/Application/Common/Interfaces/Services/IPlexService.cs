using PlexRipper.Application.Common.Models;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<bool> IsPlexAccountValid(string username, string password);
        Task<List<PlexServer>> GetServers(Account account, bool refresh = false);
        Task<string> GetPlexToken(Account account);
        PlexAccount GetPlexAccount(long plexAccountId);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        PlexAccount ConvertToPlexAccount(Account account);

        Task<PlexContainer> GetLibrary(PlexServer plexServer);
        Task<PlexAccount> RequestPlexAccountAsync(string username, string password);
        Task<PlexAccount> AddOrUpdatePlexAccount(Account account, PlexAccount plexAccountDto);
    }
}
