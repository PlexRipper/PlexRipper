using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public class DownloadHub : Hub<IDownloadHub>, IDownloadHub
{
    private readonly ILogger _log;

    /// <summary>
    ///  Initializes a new instance of the <see cref="DownloadHub"/> class.
    /// </summary>
    /// <param name="log"> The <see cref="Serilog.ILogger"/> instance to use for logging.</param>
    public DownloadHub(ILogger log)
    {
        _log = log.ForContext<DownloadHub>();
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _log.Here().Debug("Client connected to {HubName}: {ConnectionId}", nameof(DownloadHub), Context.ConnectionId);
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _log.Here()
                .Error(
                    exception,
                    "Client disconnected with error from {HubName}: {ConnectionId}",
                    nameof(DownloadHub),
                    Context.ConnectionId
                );
        }
        else
        {
            _log.Here()
                .Debug("Client disconnected from {HubName}: {ConnectionId}", nameof(DownloadHub), Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <inheritdoc/>
    public async Task ServerDownloadProgress(
        ServerDownloadProgressMessagePackDTO messagePackDTO,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@ServerDownloadProgress}",
                nameof(MessageTypes.ServerDownloadProgress),
                messagePackDTO
            );
        await Clients.All.ServerDownloadProgress(messagePackDTO, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@DownloadTaskUpdate}",
                nameof(MessageTypes.DownloadTaskUpdate),
                downloadTask
            );
        await Clients.All.DownloadTaskUpdate(downloadTask, cancellationToken);
    }
}
