using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Data.Common.Interfaces;
using PlexRipper.Domain.Entities;
using Serilog;

namespace PlexRipper.Data.Repositories
{
    public class DownloadTaskRepository : Repository<DownloadTask>, IDownloadTaskRepository
    {
        public DownloadTaskRepository(IPlexRipperDbContext context, ILogger log) : base(context, log) { }

    }
}
