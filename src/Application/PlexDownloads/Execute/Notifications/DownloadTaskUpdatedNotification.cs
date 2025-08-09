using Application.Contracts;
using Data.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public record DownloadTaskUpdatedNotification(DownloadTaskKey Key) : ICommand<Result>;

public class DownloadTaskUpdatedHandler : ICommandHandler<DownloadTaskUpdatedNotification, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task<Result> ExecuteAsync(
        DownloadTaskUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = notification.Key.PlexServerId;

        // Ensure the up-to-date download status is written to the database as the DownloadQueue depends on that status to pick a new DownloadTask
        await _dbContext.DetermineDownloadStatus(notification.Key, cancellationToken);

        var downloadTasks = await _dbContext.GetAllDownloadTasksByServerAsync(
            plexServerId,
            cancellationToken: cancellationToken
        );

        // Update the front-end with the download progress
        await _signalRService.SendDownloadProgressUpdateAsync(downloadTasks, cancellationToken);

        var changedDownloadTask = await _dbContext.GetDownloadTaskAsync(notification.Key, cancellationToken);
        if (changedDownloadTask is null)
        {
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), notification.Key.ToString()).LogError();
        }

        return Result.Ok();
    }
}
