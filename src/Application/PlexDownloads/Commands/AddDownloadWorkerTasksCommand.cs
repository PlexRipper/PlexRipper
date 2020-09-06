using System.Collections.Generic;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class AddDownloadWorkerTasksCommand : IRequest<Result<bool>>
    {
        public List<DownloadWorkerTask> DownloadWorkerTasks { get; }

        public AddDownloadWorkerTasksCommand(List<DownloadWorkerTask> downloadWorkerTasks)
        {
            DownloadWorkerTasks = downloadWorkerTasks;
        }
    }

    public class AddDownloadWorkerTasksCommandValidator : AbstractValidator<AddDownloadWorkerTasksCommand>
    {
        public AddDownloadWorkerTasksCommandValidator()
        {
            RuleForEach(x => x.DownloadWorkerTasks).ChildRules(task =>
            {
                task.RuleFor(x => x.Id).Equal(0);
                task.RuleFor(x => x.DownloadTaskId).GreaterThan(0);
                task.RuleFor(x => x.FileName).NotEmpty();
                task.RuleFor(x => x.Url).NotEmpty();
                task.RuleFor(x => x.TempDirectory).NotEmpty();
            });
        }
    }

    public class AddDownloadWorkerTasksCommandHandler : BaseHandler, IRequestHandler<AddDownloadWorkerTasksCommand, Result<bool>>
    {
        public AddDownloadWorkerTasksCommandHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(AddDownloadWorkerTasksCommand command,
            CancellationToken cancellationToken)
        {
            command.DownloadWorkerTasks.ForEach(x => x.DownloadTask = null);
            await _dbContext.DownloadWorkerTasks.AddRangeAsync(command.DownloadWorkerTasks);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}