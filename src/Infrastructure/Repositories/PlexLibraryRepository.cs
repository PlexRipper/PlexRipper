using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Persistence;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Infrastructure.Common.Interfaces;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexLibraryRepository : Repository<PlexLibrary>, IPlexLibraryRepository
    {
        public PlexLibraryRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }

        public async Task<IEnumerable<PlexLibrary>> GetLibraries(int serverId)
        {
            return await FindAllAsync(x => x.PlexServerId == serverId);
        }
    }
}
