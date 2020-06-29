using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain;

namespace PlexRipper.Data.Repositories
{
    public class PlexServerRepository : Repository<PlexServer>, IPlexServerRepository
    {
        public PlexServerRepository(IPlexRipperDbContext context) : base(context) { }

        public async Task<IEnumerable<PlexServer>> GetServers(int plexAccountId)
        {
            return await Context.PlexServers
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexAccount)
                .Where(x => x.PlexAccountServers
                    .Any(y => y.PlexAccountId == plexAccountId))
                .ToListAsync();
        }
    }
}
