using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDownloadQueue
    {
        Subject<List<DownloadTask>> UpdateDownloadTasks { get; }

        Subject<DownloadTask> StartDownloadTask { get; }

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        Task CheckDownloadQueue();
    }
}