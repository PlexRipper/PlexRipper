using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.Repositories
{
    public interface IPlexServerRepository : IRepository<PlexServer>
    {
        Task<IEnumerable<PlexServer>> GetServers(int plexAccountId);
    }
}
