using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IDownloadCommands
    {
        /// <summary>
        /// Restart the <see cref="DownloadTask"/> by deleting the PlexDownloadClient and starting a new one.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to restart.</param>
        /// <returns>Is successful.</returns>
        Task<Result> RestartDownloadTasksAsync(int downloadTaskId);

        /// <summary>
        /// Starts a queued task immediately.
        /// </summary>
        /// <param name="downloadTaskId">The ids of the <see cref="DownloadTask"/> to start.</param>
        /// <returns>Is successful.</returns>
        Task<Result> ResumeDownloadTasksAsync(int downloadTaskId);

        /// <summary>
        /// Pause a currently downloading <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to pause.</param>
        /// <returns>Is successful.</returns>
        Task<Result> PauseDownload(int downloadTaskId);

        Task<Result> StartDownloadTaskAsync(DownloadTask downloadTask);

        /// <summary>
        /// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTask"/> if it is downloading.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to stop.</param>
        /// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
        Task<Result<List<int>>> StopDownloadTasksAsync(int downloadTaskId);

        /// <summary>
        /// Will clear any completed <see cref="DownloadTask"/> from the database.
        /// </summary>
        /// <param name="downloadTaskIds"></param>
        /// <returns>Is successful.</returns>
        Task<Result> ClearCompletedAsync(List<int> downloadTaskIds = null);

        /// <summary>
        /// Stops and deletes (active) PlexDownloadClients and removes <see cref="DownloadTask"/> from the database.
        /// </summary>
        /// <param name="downloadTaskIds">The list of <see cref="DownloadTask"/> to delete.</param>
        /// <returns><see cref="Result"/> fails on error.</returns>
        Task<Result<bool>> DeleteDownloadTaskClientsAsync(List<int> downloadTaskIds);
    }
}