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
    public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
    {
        public GetDownloadTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetDownloadTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

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