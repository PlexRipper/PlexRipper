using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.FileManager
{
    public class DeleteFileTaskByIdValidator : AbstractValidator<DeleteFileTaskByIdCommand>
    {
        public DeleteFileTaskByIdValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class DeleteFileTaskByIdHandler : BaseHandler, IRequestHandler<DeleteFileTaskByIdCommand, Result>
    {
        public DeleteFileTaskByIdHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(DeleteFileTaskByIdCommand command, CancellationToken cancellationToken)
        {
            var downloadFileTask = await _dbContext.FileTasks.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (downloadFileTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(DownloadFileTask), command.Id);
            }

            _dbContext.FileTasks.Remove(downloadFileTask);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}