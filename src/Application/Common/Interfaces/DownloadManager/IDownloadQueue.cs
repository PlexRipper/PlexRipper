using System.Collections.Generic;
using System.Reactive.Subjects;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDownloadQueue
    {
        Subject<DownloadTask> UpdateDownloadTask { get; }

        Subject<int> StartDownloadTask { get; }

        void ExecuteDownloadQueue(List<PlexServer> plexServers);
    }
}