using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexServerService
    {
        Task<List<PlexServer>> GetServersAsync(PlexAccount plexAccount, bool refresh = false);
        Task<List<PlexServer>> AddServersAsync(PlexAccount plexAccount, List<PlexServer> servers);
        Task AddOrUpdatePlexServersAsync(PlexAccount plexAccount, List<PlexServer> servers);
        Task<bool> RefreshPlexServersAsync(PlexAccount plexAccount);

        Task<PlexServer> GetAllLibraryMediaAsync(PlexServer plexServer, bool refresh = false);

        Task<PlexServer> GetServerAsync(int plexServerId);
    }
}
