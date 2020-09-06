using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id, bool includeServer = false, bool includePlexAccount = false, bool includePlexLibrary = false)
        {
            Id = id;
            IncludeServer = includeServer;
            IncludePlexAccount = includePlexAccount;
            IncludePlexLibrary = includePlexLibrary;
        }

        public int Id { get; }
        public bool IncludeServer { get; }
        public bool IncludePlexAccount { get; }
        public bool IncludePlexLibrary { get; }
    }

    public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
    {
        public GetDownloadTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetDownloadTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.DownloadTasks.AsQueryable();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludePlexAccount)
            {
                query = query.Include(x => x.PlexAccount);
            }

            if (request.IncludePlexLibrary)
            {
                query = query.Include(x => x.PlexLibrary);
            }

            var downloadTask = await query
                .Include(x => x.DownloadWorkerTasks)
                .Include(x => x.DownloadFolder)
                .Include(x => x.DestinationFolder)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (downloadTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(DownloadTask), request.Id);
            }

            return Result.Ok(downloadTask);
        }
    }
}