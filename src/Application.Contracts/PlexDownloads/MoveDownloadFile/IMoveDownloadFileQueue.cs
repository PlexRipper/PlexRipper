using FluentResults;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public interface IMoveDownloadFileQueue
{
    /// <summary>
    /// Will check for any downloadTask that has finished downloading and start a moveDownloadJob for it.
    /// </summary>
    /// <returns> Result with the DownloadTaskKey that was started or a warning if no DownloadTask was found. </returns>
    Task<Result<DownloadTaskKey>> CheckMoveDownloadFileJobQueue();
}
