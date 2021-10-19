using System.Collections.Generic;
using System.Reactive.Subjects;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDownloadQueue
    {
        Subject<List<DownloadTask>> UpdateDownloadTasks { get; }

        Subject<DownloadTask> StartDownloadTask { get; }

        void ExecuteDownloadQueue(List<PlexServer> plexServers);
    }
}