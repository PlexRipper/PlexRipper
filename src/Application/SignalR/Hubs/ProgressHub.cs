using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

/// <summary>
///  The ProgressHub class is a SignalR hub that sends progress updates to the front-end.
/// </summary>
public class ProgressHub : Hub<IProgressHub>, IProgressHub
{
    private readonly ILogger _log;

    /// <summary>
    ///  Initializes a new instance of the <see cref="ProgressHub"/> class.
    /// </summary>
    /// <param name="log"> The <see cref="Serilog.ILogger"/> instance to use for logging.</param>
    public ProgressHub(ILogger log)
    {
        _log = log.ForContext<ProgressHub>();
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _log.Here().Debug("Client connected to {HubName}: {ConnectionId}", nameof(ProgressHub), Context.ConnectionId);
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
                    nameof(ProgressHub),
                    Context.ConnectionId
                );
        }
        else
        {
            _log.Here()
                .Debug("Client disconnected from {HubName}: {ConnectionId}", nameof(ProgressHub), Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <inheritdoc/>
    public async Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@JobStatusUpdateDto}",
                nameof(MessageTypes.JobStatusUpdate),
                jobStatusUpdate
            );
        await Clients.All.JobStatusUpdate(jobStatusUpdate, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@ServerConnectionCheckStatusProgress}",
                nameof(MessageTypes.ServerConnectionCheckStatusProgress),
                serverConnectionCheckStatusProgress
            );
        await Clients.All.ServerConnectionCheckStatusProgress(serverConnectionCheckStatusProgress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@LibraryProgress}",
                nameof(MessageTypes.LibraryProgress),
                libraryProgress
            );
        await Clients.All.LibraryProgress(libraryProgress, cancellationToken);
    }
}
