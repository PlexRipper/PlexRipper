using Application.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
///  The ProgressHub class is a SignalR hub that sends progress updates to the front-end.
/// </summary>
public interface IProgressHub
{
    /// <summary>
    ///  Sends a background job status update to the front-end.
    /// </summary>
    /// <param name="jobStatusUpdate"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Sends a sync server media progress update to the front-end.
    /// </summary>
    /// <param name="syncServerMediaProgress"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task SyncServerMediaProgress(
        SyncServerMediaProgress syncServerMediaProgress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///  Sends a media file merge progress update to the front-end.
    /// </summary>
    /// <param name="fileMergeProgress"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task FileMergeProgress(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Sends a server connection check status progress update to the front-end.
    /// </summary>
    /// <param name="serverConnectionCheckStatusProgress"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    );

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

    /// <summary>
    ///  Sends a library progress update to the front-end.
    /// </summary>
    /// <param name="libraryProgress"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default);
}