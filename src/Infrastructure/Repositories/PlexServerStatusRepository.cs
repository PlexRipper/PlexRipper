using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.Interfaces;
using Serilog;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexServerStatusRepository : Repository<PlexServerStatus>, IPlexServerStatusRepository
    {
        public PlexServerStatusRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }

    }
}
