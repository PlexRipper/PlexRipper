using System.Collections.Generic;
using System.Reactive.Subjects;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDownloadQueue
    {
        Subject<DownloadClientUpdate> UpdateDownloadClient { get; }

        Subject<int> StartDownloadTask { get; }

        void ExecuteDownloadQueue(List<PlexServer> plexServers);
    }
}