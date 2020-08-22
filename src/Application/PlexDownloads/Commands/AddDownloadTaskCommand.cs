using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class AddDownloadTaskCommand : IRequest<Result<int>>
    {
        public DownloadTask DownloadTask { get; }

        public AddDownloadTaskCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }

    public class AddDownloadTaskCommandValidator : AbstractValidator<AddDownloadTaskCommand>
    {
        public AddDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTask.Id).Equal(0);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.FolderPathId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.PlexAccountId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.FileName).NotEmpty();
        }
    }

    public class AddDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddDownloadTaskCommand, Result<int>>
    {
        public AddDownloadTaskCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<int>> Handle(AddDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            await _dbContext.DownloadTasks.AddAsync(command.DownloadTask, cancellationToken);

            if (command.DownloadTask.FolderPath != null)
            {
                _dbContext.Entry(command.DownloadTask.FolderPath).State = EntityState.Unchanged;
            }

            if (command.DownloadTask.PlexServer != null)
            {
                _dbContext.Entry(command.DownloadTask.PlexServer).State = EntityState.Unchanged;
            }

            if (command.DownloadTask.PlexAccount != null)
            {
                _dbContext.Entry(command.DownloadTask.PlexAccount).State = EntityState.Unchanged;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.DownloadTask).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(command.DownloadTask.Id);
        }
    }
}
