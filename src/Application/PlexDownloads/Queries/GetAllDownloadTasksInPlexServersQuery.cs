using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    /// <summary>
    /// Request all downloadTasks sorted in their respective <see cref="PlexServer"/> and <see cref="PlexLibrary"/>
    /// </summary>
    public class GetAllDownloadTasksInPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllDownloadTasksInPlexServersQuery(bool includePlexAccount = false, bool includeServerStatus = false)
        {
            IncludeServerStatus = includeServerStatus;
            IncludePlexAccount = includePlexAccount;
        }

        public bool IncludeServerStatus { get; }
        public bool IncludePlexAccount { get; }
    }

    public class GetAllDownloadTasksInPlexServersQueryValidator : AbstractValidator<GetAllDownloadTasksInPlexServersQuery>
    {
        public GetAllDownloadTasksInPlexServersQueryValidator() { }
    }


    public class GetAllDownloadTasksInPlexServersQueryHandler : BaseHandler,
        IRequestHandler<GetAllDownloadTasksInPlexServersQuery, Result<List<PlexServer>>>
    {
        public GetAllDownloadTasksInPlexServersQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

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