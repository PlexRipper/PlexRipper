using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public interface ISignalRService
{
    /// <summary>
    ///  Sends a background job status update to the front-end.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    Task SendLibraryProgressUpdateAsync(LibraryProgress progress);

    /// <summary>
    ///  Sends a notification to the front-end.
    /// </summary>
    /// <param name="notification"> The <see cref="Notification"/> to send.</param>
    /// <returns></returns>
    Task SendNotificationAsync(Notification notification);

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
    /// <param name="refreshDataType"></param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task SendRefreshNotificationAsync(RefreshDataType refreshDataType, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Sends a refresh data notification to the front-end.
    /// </summary>
    /// <param name="dataType"> The <see cref="RefreshDataType"/> to send.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task SendRefreshNotificationAsync(List<RefreshDataType> dataType, CancellationToken cancellationToken = default);
}
