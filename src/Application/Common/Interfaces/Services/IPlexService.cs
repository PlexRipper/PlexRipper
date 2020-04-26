using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<PlexAccount> IsAccountValid(Account account);
        Task<string> GetPlexToken(Account account);
        Task<List<string>> GetServers(Account account);
        PlexAccount AddOrUpdatePlexAccount(PlexAccountDTO plexAccountDto);
        PlexAccount GetPlexAccount(long plexAccountId);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        PlexAccount ConvertToPlexAccount(Account account);
    }
}
