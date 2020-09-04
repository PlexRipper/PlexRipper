using System.Collections.Generic;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class AddDownloadWorkerTaskCommand : IRequest<Result<bool>>
    {
        public List<DownloadWorkerTask> DownloadWorkerTasks { get; }

        public AddDownloadWorkerTaskCommand(List<DownloadWorkerTask> downloadWorkerTasks)
        {
            DownloadWorkerTasks = downloadWorkerTasks;
        }
    }

    public class AddDownloadWorkerTaskCommandValidator : AbstractValidator<AddDownloadWorkerTaskCommand>
    {
        public AddDownloadWorkerTaskCommandValidator()
        {
            RuleForEach(x => x.DownloadWorkerTasks).ChildRules(task =>
            {
                task.RuleFor(x => x.Id).Equal(0);
                task.RuleFor(x => x.DownloadTaskId).GreaterThan(0);
                task.RuleFor(x => x.FileName).NotEmpty();
                task.RuleFor(x => x.Url).NotEmpty();
                task.RuleFor(x => x.DownloadDirectory).NotEmpty();
            });
        }
    }

    public class AddDownloadWorkerTaskCommandHandler : BaseHandler, IRequestHandler<AddDownloadWorkerTaskCommand, Result<bool>>
    {
        public AddDownloadWorkerTaskCommandHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(AddDownloadWorkerTaskCommand command,
            CancellationToken cancellationToken)
        {
            command.DownloadWorkerTasks.ForEach(x => x.DownloadTask = null);
            await _dbContext.DownloadWorkerTasks.AddRangeAsync(command.DownloadWorkerTasks);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}