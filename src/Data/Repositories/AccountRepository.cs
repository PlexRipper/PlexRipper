using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(IPlexRipperDbContext context) : base(context) { }

    }
}
