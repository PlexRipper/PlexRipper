using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class AddDownloadTaskCommandCommand : IRequest<Result<DownloadTask>>
    {
        public DownloadTask DownloadTask { get; }

        public AddDownloadTaskCommandCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }

    public class AddDownloadTaskCommandValidator : AbstractValidator<AddDownloadTaskCommandCommand>
    {
        public AddDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTask.Id).Equal(0);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DownloadTask.FileName).NotEmpty();
            RuleFor(x => x.DownloadTask.FolderPathId).GreaterThanOrEqualTo(0);
        }
    }

    public class AddDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddDownloadTaskCommandCommand, Result<DownloadTask>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public AddDownloadTaskCommandHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<DownloadTask>> Handle(AddDownloadTaskCommandCommand command, CancellationToken cancellationToken)
        {
            await _dbContext.DownloadTasks.AddAsync(command.DownloadTask);
            if (command.DownloadTask.FolderPath != null)
            {
                _dbContext.Entry(command.DownloadTask.FolderPath).State = EntityState.Unchanged;
            }
            if (command.DownloadTask.PlexServer != null)
            {
                _dbContext.Entry(command.DownloadTask.PlexServer).State = EntityState.Unchanged;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(command.DownloadTask).GetDatabaseValuesAsync(cancellationToken);

            return ReturnResult(command.DownloadTask, command.DownloadTask.Id);
        }
    }
}
