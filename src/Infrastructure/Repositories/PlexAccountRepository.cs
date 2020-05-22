using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Persistence;
using Serilog;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexAccountRepository : Repository<PlexAccount>, IPlexAccountRepository
    {
        public PlexAccountRepository(PlexRipperDbContext context, ILogger log) : base(context, log) { }

    }
}
