using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class GetDownloadTasksByPlexServerIdQueryValidator : AbstractValidator<GetDownloadTasksByPlexServerIdQuery>
    {
        public GetDownloadTasksByPlexServerIdQueryValidator()
        {
            RuleFor(x => x.PlexServerId).GreaterThan(0);
        }
    }

    public class GetDownloadTasksByPlexServerIdQueryHandler : BaseHandler,
        IRequestHandler<GetDownloadTasksByPlexServerIdQuery, Result<PlexServer>>
    {
        public GetDownloadTasksByPlexServerIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetDownloadTasksByPlexServerIdQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlexServer> query = _dbContext.PlexServers
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ThenInclude(x => x.PlexServer);

            if (request.IncludePlexAccount)
            {
                query = query
                    .Include(x => x.PlexLibraries)
                    .ThenInclude(x => x.DownloadTasks)
                    .ThenInclude(x => x.PlexAccount);
            }

            if (request.IncludeServerStatus)
            {
                query = query.Include(x => x.ServerStatus);
            }

            var server = await query

                // Include DownloadWorkerTasks
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ThenInclude(x => x.DownloadWorkerTasks)

                // Include DownloadFolder
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ThenInclude(x => x.DownloadFolder)

                // Include DestinationFolder
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ThenInclude(x => x.DestinationFolder)
                .FirstOrDefaultAsync(x => x.Id == request.PlexServerId);
            return Result.Ok(server);
        }
    }
}