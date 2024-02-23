using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadCommands
{
    /// <summary>
    /// Stops and deletes (active) PlexDownloadClients and removes <see cref="DownloadTask"/> from the database.
    /// </summary>
    /// <param name="downloadTaskIds">The list of <see cref="DownloadTask"/> to delete.</param>
    /// <returns><see cref="Result"/> fails on error.</returns>
    Task<Result<bool>> DeleteDownloadTaskClients(List<int> downloadTaskIds);
}