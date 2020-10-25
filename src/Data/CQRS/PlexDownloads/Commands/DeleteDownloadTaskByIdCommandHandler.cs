using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class DeleteDownloadTaskByIdCommandValidator : AbstractValidator<DeleteDownloadTaskByIdCommand>
    {
        public DeleteDownloadTaskByIdCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
        }
    }

    public class DeleteDownloadTaskByIDHandler : BaseHandler, IRequestHandler<DeleteDownloadTaskByIdCommand, Result<bool>>
    {
        public DeleteDownloadTaskByIDHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(DeleteDownloadTaskByIdCommand command, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.DownloadTasks.AsTracking().FirstOrDefaultAsync(x => x.Id == command.DownloadTaskId, cancellationToken);
            if (entity == null)
            {
                Log.Warning($"The entity of type DownloadTask with id {command.DownloadTaskId} could not be found");
                return Result.Ok(false);
            }

            _dbContext.DownloadTasks.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}