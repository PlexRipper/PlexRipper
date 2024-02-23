using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadCommands
{
    /// <summary>
    /// Pause a currently downloading <see cref="DownloadTask"/>.
    /// </summary>
    /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to pause.</param>
    /// <returns>Is successful.</returns>
    Task<Result> PauseDownloadTask(int downloadTaskId);

    /// <summary>
    /// Stops and deletes (active) PlexDownloadClients and removes <see cref="DownloadTask"/> from the database.
    /// </summary>
    /// <param name="downloadTaskIds">The list of <see cref="DownloadTask"/> to delete.</param>
    /// <returns><see cref="Result"/> fails on error.</returns>
    Task<Result<bool>> DeleteDownloadTaskClients(List<int> downloadTaskIds);
}