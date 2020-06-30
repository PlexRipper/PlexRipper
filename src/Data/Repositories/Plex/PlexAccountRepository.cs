using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class PlexAccountRepository : Repository<PlexAccount>, IPlexAccountRepository
    {
        public PlexAccountRepository(IPlexRipperDbContext context) : base(context) { }

    }
}
