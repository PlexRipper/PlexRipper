using MediatR;

namespace DownloadManager.Contracts;

public class DownloadTaskFinished : INotification
{
    #region Constructors

    public DownloadTaskFinished(int downloadTaskId, int plexServerId)
    {
        DownloadTaskId = downloadTaskId;
        PlexServerId = plexServerId;
    }

    #endregion

    #region Properties

    public int DownloadTaskId { get; }
    public int PlexServerId { get; }

    #endregion
}