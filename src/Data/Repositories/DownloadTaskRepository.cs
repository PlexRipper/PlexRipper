using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Repositories
{
    public class DownloadTaskRepository : Repository<DownloadTask>, IDownloadTaskRepository
    {
        public DownloadTaskRepository(IPlexRipperDbContext context) : base(context) { }

    }
}
