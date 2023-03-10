using MediatR;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadTaskUpdated : IRequest
{
    #region Constructors

    public DownloadTaskUpdated(DownloadTask downloadTask)
    {
        DownloadTaskId = downloadTask.Id;
        PlexServerId = downloadTask.PlexServerId;
        RootDownloadTaskId = downloadTask.RootDownloadTaskId;
    }

    public DownloadTaskUpdated(int downloadTaskId, int plexServerId, int rootDownloadTaskId)
    {
        DownloadTaskId = downloadTaskId;
        PlexServerId = plexServerId;
        RootDownloadTaskId = rootDownloadTaskId;
    }

    #endregion

    #region Properties

    public int DownloadTaskId { get; }
    public int PlexServerId { get; }
    public int RootDownloadTaskId { get; }

    #endregion
}