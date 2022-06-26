using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadTaskValidator
    {
        Task<Result<List<DownloadTask>>> CheckIfDownloadTasksExist(List<DownloadTask> downloadTasks);

        Result<List<DownloadTask>> ValidateDownloadTasks(List<DownloadTask> downloadTasks);

        /// <summary>
        /// Validates the <see cref="DownloadTask"/>s and returns only the valid one while notifying of any failed ones.
        /// Returns only a failed result when all downloadTasks failed validation.
        /// </summary>
        /// <param name="downloadTasks">The <see cref="DownloadTask"/>s to validate.</param>
        /// <returns>Only the valid <see cref="DownloadTask"/>s.</returns>
        Task<Result<List<DownloadTask>>> VerifyDownloadTasks(List<DownloadTask> downloadTasks);
    }
}