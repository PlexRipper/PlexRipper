namespace Reaparr.Application.Contracts;

public interface IDownloadQueue : ISetup, IBusy
{
    /// <summary>
    /// Check the DownloadQueue for downloadTasks which can be started.
    /// </summary>
    Task<Result> CheckDownloadQueue(List<int> plexServerIds);

    /// <summary>
    /// Resets any download tasks still in <see cref="DownloadStatus.Downloading"/> back to
    /// <see cref="DownloadStatus.Queued"/>. A task can be left in <see cref="DownloadStatus.Downloading"/>
    /// when the process is killed mid-download (OOM, container restart) and the
    /// <c>DownloadFileCompleted</c> handler never fires. Without this sweep on boot, the queue
    /// picker treats the zombie as an active download and never picks a new task for that server.
    /// Persisted progress (offsets, snapshot) is preserved so the next attempt resumes from where
    /// it left off.
    /// </summary>
    Task<Result> RecoverInterruptedDownloadsAsync(CancellationToken cancellationToken = default);
}
