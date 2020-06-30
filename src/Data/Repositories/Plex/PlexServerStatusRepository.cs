using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Data.Repositories
{
    public class PlexServerStatusRepository : Repository<PlexServerStatus>, IPlexServerStatusRepository
    {
        public PlexServerStatusRepository(IPlexRipperDbContext context) : base(context) { }


        public override Task AddAsync(PlexServerStatus entity)
        {
            //Context.Instance.Set<PlexServer>().Update(entity.PlexServer);
            return base.AddAsync(entity);
        }
    }
}
