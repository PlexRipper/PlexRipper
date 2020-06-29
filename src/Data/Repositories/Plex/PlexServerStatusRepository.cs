using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain;

namespace PlexRipper.Data.Repositories
{
    public class PlexServerStatusRepository : Repository<PlexServerStatus>, IPlexServerStatusRepository
    {
        public PlexServerStatusRepository(IPlexRipperDbContext context) : base(context) { }

    }
}
