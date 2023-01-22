using MediatR;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadStatusChanged : INotification
{
    #region Constructors

    public DownloadStatusChanged(int downloadTaskId, int rootDownloadTaskId, DownloadStatus status)
    {
        DownloadTaskId = downloadTaskId;
        RootDownloadTaskId = rootDownloadTaskId;
        Status = status;
    }

    #endregion

    #region Properties

    public int DownloadTaskId { get; }
    public int RootDownloadTaskId { get; }
    public DownloadStatus Status { get; }

    #endregion
}