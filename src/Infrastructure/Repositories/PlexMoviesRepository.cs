using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.Interfaces;
using Serilog;

namespace PlexRipper.Infrastructure.Repositories
{
    public class PlexMoviesRepository : Repository<PlexMovie>, IPlexMoviesRepository
    {
        public PlexMoviesRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }


    }
}
