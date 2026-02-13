using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.SignalR;

/// <summary>
/// Sends download-related SignalR messages to the front-end via <see cref="DownloadHub"/>.
/// </summary>
public class DownloadHubService : IDownloadHubService
{
    private readonly ILogger _log;
    private readonly IHubContext<DownloadHub, IDownloadHub> _hub;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadHubService"/> class.
    /// </summary>
    public DownloadHubService(ILogger log, IHubContext<DownloadHub, IDownloadHub> hub)
    {
        _log = log.ForContext<DownloadHubService>();
        _hub = hub;
    }

    /// <inheritdoc/>
    public async Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    )
    {
        var update = downloadTasks.ToServerDownloadProgressDTOList();
        if (!update.Any())
        {
            _log.Here().Error("Update for ServerDownloadProgress contained no entries to be sent");
            return;
        }

        var messagePack = update.First().ToMessagePack();

        await _hub.Clients.All.ServerDownloadProgress(messagePack, cancellationToken);
    }
}
