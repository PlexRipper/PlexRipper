using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.Interfaces;
using Serilog;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexAccountRepository : Repository<PlexAccount>, IPlexAccountRepository
    {
        public PlexAccountRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }

    }
}
