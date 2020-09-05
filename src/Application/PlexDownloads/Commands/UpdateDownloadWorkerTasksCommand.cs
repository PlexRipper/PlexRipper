using System.Collections.Generic;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class UpdateDownloadWorkerTasksCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadWorkerTasksCommand(IList<DownloadWorkerTask> downloadTasks)
        {
            DownloadTasks = downloadTasks;
        }

        public IList<DownloadWorkerTask> DownloadTasks { get; }
    }

    public class UpdateDownloadWorkerTasksCommandValidator : AbstractValidator<UpdateDownloadWorkerTasksCommand>
    {
        public UpdateDownloadWorkerTasksCommandValidator()
        {
            RuleFor(x => x.DownloadTasks).NotNull();
            RuleFor(x => x.DownloadTasks.Count).GreaterThan(0);
            RuleForEach(x => x.DownloadTasks).ChildRules(downloadTask => {
                downloadTask.RuleFor(x => x.Id).GreaterThan(0);
                downloadTask.RuleFor(x => x.BytesReceived).GreaterThan(0);
                downloadTask.RuleFor(x => x.DownloadTaskId).GreaterThan(0);
            });
        }
    }

    public class UpdateDownloadWorkerTasksCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadWorkerTasksCommand, Result<bool>>
    {
        public UpdateDownloadWorkerTasksCommandHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(UpdateDownloadWorkerTasksCommand command, CancellationToken cancellationToken)
        {
            _dbContext.DownloadWorkerTasks.UpdateRange(command.DownloadTasks);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(true);
        }
    }
}