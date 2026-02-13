namespace Reaparr.SignalR.Contracts;

public interface IDownloadHubService
{
    /// <summary>
    /// Sends a download progress update to the front-end.
    /// </summary>
    Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    );
}
