using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexServerService
    {
        Task<List<PlexServer>> GetServers(PlexAccount plexAccount, bool refresh = false);
        Task<List<PlexServer>> AddServers(PlexAccount plexAccount, List<PlexServer> servers);
        Task AddOrUpdatePlexServersAsync(PlexAccount plexAccount, List<PlexServer> servers);
        Task<bool> RefreshPlexServersAsync(PlexAccount plexAccount);
    }
}
