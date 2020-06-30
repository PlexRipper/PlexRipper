using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.Repositories
{
    public interface IPlexServerRepository : IRepository<PlexServer>
    {
        Task<IEnumerable<PlexServer>> GetServers(int plexAccountId);
    }
}
