using MediatR;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadTaskUpdated : INotification
{
    #region Constructors

    public DownloadTaskUpdated(DownloadTask downloadTask)
    {
        DownloadTask = downloadTask;
    }

    #endregion

    #region Properties

    public DownloadTask DownloadTask { get; }

    #endregion
}