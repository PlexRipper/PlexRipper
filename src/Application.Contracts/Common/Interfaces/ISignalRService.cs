using PlexRipper.Domain;
using WebAPI.Contracts;

namespace Application.Contracts;

public interface ISignalRService
{
    /// <summary>
    ///  Sends a background job status update to the front-end.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    Task SendLibraryProgressUpdateAsync(LibraryProgress progress);

    /// <summary>
    /// Sends a <see cref="FileMergeProgress"/> object to the SignalR client in the front-end.
    /// </summary>
    /// <param name="fileMergeProgress">The <see cref="FileMergeProgress"/> object to send.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    Task SendFileMergeProgressUpdateAsync(
        FileMergeProgress fileMergeProgress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///  Sends a notification to the front-end.
    /// </summary>
    /// <param name="notification"> The <see cref="Notification"/> to send.</param>
    /// <returns></returns>
    Task SendNotificationAsync(Notification notification);

    /// <summary>
    ///  Sends a sync server media progress update to the front-end.
    /// </summary>
    /// <param name="syncServerMediaProgress"></param>
    /// <returns></returns>
    Task SendServerSyncProgressUpdateAsync(SyncServerMediaProgress syncServerMediaProgress);

    /// <summary>
    ///  Sends a download progress update to the front-end.
    /// </summary>
    /// <param name="downloadTasks"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a server connection check status progress update to the front-end.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress);

    /// <summary>
    ///  Sends a job status update to the front-end.
    /// </summary>
    /// <param name="jobStatusUpdate"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class;

    /// <summary>
    ///  Sends a refresh notification to the front-end.
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task SendRefreshNotificationAsync(DataType dataType, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Sends a refresh data notification to the front-end.
    /// </summary>
    /// <param name="dataType"> The <see cref="DataType"/> to send.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task SendRefreshNotificationAsync(List<DataType> dataType, CancellationToken cancellationToken = default);
}
