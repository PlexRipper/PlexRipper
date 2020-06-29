using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain;

namespace PlexRipper.Data.Repositories
{
    public class PlexAccountRepository : Repository<PlexAccount>, IPlexAccountRepository
    {
        public PlexAccountRepository(IPlexRipperDbContext context) : base(context) { }

    }
}
