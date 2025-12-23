using FastEndpoints;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record ServerOnlineStatusChangedNotification : IEvent
{
    public ServerOnlineStatusChangedNotification(int plexServerId, bool isOnline)
    {
        PlexServerId = plexServerId;
        IsOnline = isOnline;
    }

    public int PlexServerId { get; }

    public bool IsOnline { get; }
}

public class ServerOnlineStatusChangedHandler : IEventHandler<ServerOnlineStatusChangedNotification>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadQueue _downloadQueue;
    private readonly ICommandExecutor _commandExecutor;

    public ServerOnlineStatusChangedHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IDownloadQueue downloadQueue,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<ServerOnlineStatusChangedHandler>();
        _dbContext = dbContext;
        _downloadQueue = downloadQueue;
        _commandExecutor = commandExecutor;
    }

    public async Task HandleAsync(
        ServerOnlineStatusChangedNotification notification,
        CancellationToken cancellationToken
    )
    {
        if (notification.IsOnline)
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(
                notification.PlexServerId,
                cancellationToken: cancellationToken
            );
            _log.Here()
                .Information(
                    "Server {PlexServerName} came online, checking DownloadQueue and LibrarySyncQueue to resume",
                    plexServerName
                );

            await _downloadQueue.CheckDownloadQueue([notification.PlexServerId]);
            await _commandExecutor.Send(
                new ResetFailedLibrarySyncJobsCommand(notification.PlexServerId),
                cancellationToken
            );
        }
    }
}
