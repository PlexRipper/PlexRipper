using Application.Contracts;
using Data.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// NOTE: This should be an IRequest to ensure there is always 1 handler for this notification.
/// </summary>
public record DownloadTaskUpdatedNotification(DownloadTaskKey Key) : IRequest;

public class DownloadTaskUpdatedHandler : IRequestHandler<DownloadTaskUpdatedNotification>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdatedNotification notification, CancellationToken cancellationToken)
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
            ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), notification.Key.ToString()).LogError();
        }
    }
}
