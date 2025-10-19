using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public interface IDownloadHub
{
    /// <summary>
    ///  Sends a server download progress update to the front-end.
    /// </summary>
    /// <param name="serverDownloadProgress"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task ServerDownloadProgress(
        ServerDownloadProgressDTO serverDownloadProgress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///  Sends a download task update to the front-end.
    /// </summary>
    /// <param name="downloadTask"> The <see cref="DownloadTaskDTO"/> to send.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default);
}