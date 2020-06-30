using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class PlexMoviesRepository : Repository<PlexMovie>, IPlexMoviesRepository
    {
        public PlexMoviesRepository(IPlexRipperDbContext context) : base(context) { }


    }
}
