using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    /// <summary>
    /// Request all downloadTasks sorted in their respective <see cref="PlexServer"/> and <see cref="PlexLibrary"/>
    /// </summary>
    public class GetDownloadTasksByPlexServerIdQuery : IRequest<Result<PlexServer>>
    {
        public GetDownloadTasksByPlexServerIdQuery(int plexServerId, bool includePlexAccount = false, bool includeFolderPaths = false,
            bool includeServerStatus = false)
        {
            PlexServerId = plexServerId;
            IncludeServerStatus = includeServerStatus;
            IncludePlexAccount = includePlexAccount;
            IncludeFolderPaths = includeFolderPaths;
        }

        public int PlexServerId { get; }
        public bool IncludeServerStatus { get; }
        public bool IncludePlexAccount { get; }
        public bool IncludeFolderPaths { get; }
    }

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
        public GetDownloadTasksByPlexServerIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

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
            if (request.IncludeFolderPaths)
            {
                query = query
                    .Include(x => x.PlexLibraries)
                    .ThenInclude(x => x.DownloadTasks)
                    .ThenInclude(x => x.DownloadFolder)
                    .Include(x => x.PlexLibraries)
                    .ThenInclude(x => x.DownloadTasks)
                    .ThenInclude(x => x.DestinationFolder);
            }
            if (request.IncludeServerStatus)
            {
                query = query.Include(x => x.ServerStatus);
            }
            var server = await query
                .FirstOrDefaultAsync(x => x.Id == request.PlexServerId);
            return Result.Ok(server);
        }
    }
}