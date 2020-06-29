using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class PlexMoviesRepository : Repository<PlexMovie>, IPlexMoviesRepository
    {
        public PlexMoviesRepository(IPlexRipperDbContext context) : base(context) { }


    }
}
