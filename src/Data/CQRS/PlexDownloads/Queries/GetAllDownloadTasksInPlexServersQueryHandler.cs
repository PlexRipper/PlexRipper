using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class GetAllDownloadTasksInPlexServersQueryValidator : AbstractValidator<GetAllDownloadTasksInPlexServersQuery> { }

    public class GetAllDownloadTasksInPlexServersQueryHandler : BaseHandler,
        IRequestHandler<GetAllDownloadTasksInPlexServersQuery, Result<List<PlexServer>>>
    {
        public GetAllDownloadTasksInPlexServersQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexServer>>> Handle(GetAllDownloadTasksInPlexServersQuery request, CancellationToken cancellationToken)
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

            var serverList = await query

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
                .Where(x => x.PlexLibraries.Any(y => y.DownloadTasks.Any()))
                .ToListAsync(cancellationToken);
            return Result.Ok(serverList);
        }
    }
}