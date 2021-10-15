using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
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
            var downloadTask =
                await DownloadTasksQueryable
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .IncludeDownloadTasks()
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (downloadTask == null)
            {
                return ResultExtensions.EntityNotFound(nameof(DownloadTask), request.Id);
            }

            return Result.Ok(downloadTask);
        }
    }
}