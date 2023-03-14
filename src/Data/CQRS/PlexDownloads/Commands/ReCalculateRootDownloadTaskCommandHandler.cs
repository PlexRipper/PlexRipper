using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class ReCalculateRootDownloadTaskCommandHandlerValidator : AbstractValidator<ReCalculateRootDownloadTaskCommand>
{
    public ReCalculateRootDownloadTaskCommandHandlerValidator()
    {
        RuleFor(x => x.RootDownloadTaskId).GreaterThan(0);
    }
}

public class ReCalculateRootDownloadTaskCommandHandler : BaseHandler,
    IRequestHandler<ReCalculateRootDownloadTaskCommand, Result>
{
    public ReCalculateRootDownloadTaskCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(ReCalculateRootDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskDb = await DownloadTasksQueryable
            .IncludeDownloadTasks()
            .FirstOrDefaultAsync(x => x.Id == command.RootDownloadTaskId, cancellationToken);

        void SetDownloadStatus(DownloadTask downloadTask)
        {
            if (downloadTask.Children is null || !downloadTask.Children.Any())
                return;

            foreach (var downloadTaskChild in downloadTask.Children)
                SetDownloadStatus(downloadTaskChild);

            downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());
            downloadTask.DownloadSpeed = downloadTask.Children.Sum(x => x.DownloadSpeed);
            downloadTask.FileTransferSpeed = downloadTask.Children.Sum(x => x.FileTransferSpeed);
            downloadTask.Percentage = (int)downloadTask.Children.Average(x => x.Percentage);
            downloadTask.DataReceived = downloadTask.Children.Sum(x => x.DataReceived);
        }

        SetDownloadStatus(downloadTaskDb);

        await SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}