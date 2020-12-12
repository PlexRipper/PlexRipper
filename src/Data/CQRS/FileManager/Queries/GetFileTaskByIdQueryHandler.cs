using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.FileManager
{
    public class GetFileTaskByIdQueryValidator : AbstractValidator<GetFileTaskByIdQuery>
    {
        public GetFileTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetFileTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetFileTaskByIdQuery, Result<DownloadFileTask>>
    {
        public GetFileTaskByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<DownloadFileTask>> Handle(GetFileTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var fileTask = await _dbContext.FileTasks
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DownloadWorkerTasks)
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DownloadFolder)
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DestinationFolder)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (fileTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(DownloadFileTask), request.Id);
            }

            return Result.Ok(fileTask);
        }
    }
}