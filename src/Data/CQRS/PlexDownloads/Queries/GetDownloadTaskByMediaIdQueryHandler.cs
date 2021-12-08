using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class GetDownloadTaskByMediaIdQueryValidator : AbstractValidator<GetDownloadTaskByMediaKeyQuery>
    {
        public GetDownloadTaskByMediaIdQueryValidator()
        {
            RuleFor(x => x.MediaId).GreaterThan(0);
            RuleFor(x => x.Type).NotEqual(PlexMediaType.None).NotEqual(PlexMediaType.Unknown);
        }
    }

    public class GetDownloadTaskByMediaIdQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByMediaKeyQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByMediaIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByMediaKeyQuery request, CancellationToken cancellationToken)
        {
            var downloadTask =
                await DownloadTasksQueryable
                    .AsTracking()
                    .IncludeDownloadTasks()
                    .FirstOrDefaultAsync(x => x.Id == request.MediaId && x.MediaType == request.Type, cancellationToken);

            return ReturnResult(downloadTask, request.MediaId);
        }
    }
}