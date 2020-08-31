using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class DeleteDownloadTaskByIdCommand : IRequest<Result<bool>>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadTaskByIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }

    public class DeleteDownloadTaskByIdCommandValidator : AbstractValidator<DeleteDownloadTaskByIdCommand>
    {
        public DeleteDownloadTaskByIdCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
        }
    }

    public class DeleteDownloadTaskByIDHandler : BaseHandler, IRequestHandler<DeleteDownloadTaskByIdCommand, Result<bool>>
    {
        public DeleteDownloadTaskByIDHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<bool>> Handle(DeleteDownloadTaskByIdCommand command, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<DeleteDownloadTaskByIdCommand, DeleteDownloadTaskByIdCommandValidator>(command);
            if (result.IsFailed) return result;

            var entity = await _dbContext.DownloadTasks.AsTracking().FirstOrDefaultAsync(x => x.Id == command.DownloadTaskId, cancellationToken: cancellationToken);
            if (entity == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(DownloadTask), command.DownloadTaskId).LogError();
            }

            _dbContext.DownloadTasks.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);

        }
    }
}
