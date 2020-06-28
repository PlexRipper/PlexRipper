using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Data.Repositories
{
    public class PlexLibraryRepository : Repository<PlexLibrary>, IPlexLibraryRepository
    {
        public PlexLibraryRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }

        public async Task<IList<PlexLibrary>> GetLibraries(int serverId)
        {
            var x = await FindAllAsync(x => x.PlexServerId == serverId);
            return x.ToList();
        }
    }
}
