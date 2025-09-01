using FastEndpoints;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

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
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadQueue _downloadQueue;

    public ServerOnlineStatusChangedHandler(ILogger log, IReaparrDbContext dbContext, IDownloadQueue downloadQueue)
    {
        _log = log.ForContext<ServerOnlineStatusChangedHandler>();
        _dbContext = dbContext;
        _downloadQueue = downloadQueue;
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
                    "Server {PlexServerName} came online, checking DownloadQueue to resume downloads",
                    plexServerName
                );
            await _downloadQueue.CheckDownloadQueue([notification.PlexServerId]);
        }
    }
}
