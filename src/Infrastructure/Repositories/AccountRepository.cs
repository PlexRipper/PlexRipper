using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(PlexRipperDbContext context) : base(context) { }

        public override IQueryable<Account> BaseIncludes()
        {
            return Context.Set<Account>()
                .Include(x => x.PlexAccount)
                .ThenInclude(x => x.PlexAccountServers)
                .AsNoTracking();
        }

        public override async Task<Account> GetWithIncludeAsync(int id)
        {
            return await BaseIncludes().FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Account>> GetAllWithIncludeAsync()
        {
            return await BaseIncludes().ToListAsync();
        }

        public override async Task<Account> FindWithIncludeAsync(Expression<Func<Account, bool>> predicate)
        {
            return await BaseIncludes()
                .Where(predicate)
                .FirstOrDefaultAsync();
        }
    }
}
