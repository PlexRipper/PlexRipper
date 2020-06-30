using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class PlexSeriesRepository : Repository<PlexSerie>, IPlexSerieRepository
    {
        public PlexSeriesRepository(IPlexRipperDbContext context) : base(context) { }


    }
}
