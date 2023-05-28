using MediatR;

namespace DownloadManager.Contracts;

public class CheckDownloadQueue : INotification
{
    #region Constructors

    public CheckDownloadQueue(int plexServerId)
    {
        PlexServerIds = new List<int>() { plexServerId };
    }

    public CheckDownloadQueue(List<int> plexServerIds)
    {
        PlexServerIds = plexServerIds;
    }

    #endregion

    #region Properties

    public List<int> PlexServerIds { get; }

    #endregion
}