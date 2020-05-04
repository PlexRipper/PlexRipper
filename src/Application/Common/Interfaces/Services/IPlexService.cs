using PlexRipper.Application.Common.Models;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        /// <summary>
        /// Check the validity of <see cref="Account"/> credentials to the Plex API. 
        /// </summary>
        /// <param name="account">The Account to be validated</param>
        /// <returns>The PlexAccount in DB that is returned from the Plex API</returns>
        Task<PlexAccount> IsAccountValid(Account account);
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
    }
}
