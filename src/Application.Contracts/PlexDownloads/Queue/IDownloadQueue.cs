using FluentResults;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public interface IDownloadQueue : ISetup, IBusy
{
    /// <summary>
    /// Check the DownloadQueue for downloadTasks which can be started.
    /// </summary>
    Task<Result> CheckDownloadQueue(List<int> plexServerIds);
}
