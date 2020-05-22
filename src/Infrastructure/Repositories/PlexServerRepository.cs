using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Persistence;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexServerRepository : Repository<PlexServer>, IPlexServerRepository
    {
        public PlexServerRepository(PlexRipperDbContext context, ILogger log) : base(context, log) { }

        public async Task<IEnumerable<PlexServer>> GetServers(int plexAccountId)
        {
            return await Context.PlexServers
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexAccount)
                .Where(x => x.PlexAccountServers
                    .Any(y => y.PlexAccountId == plexAccountId))
                .ToListAsync();

            //return await Context.PlexServers
            //     .Include(p => p.PlexAccountServers)
            //     .ThenInclude(x => x.PlexServer)
            //     .Where(y => y.PlexAccountServers
            //         .Any(p => p.PlexAccountId == plexAccountId))
            //     .ToListAsync();

        }
    }
}
