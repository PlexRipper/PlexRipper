using MediatR;

namespace DownloadManager.Contracts;

public class CheckDownloadQueue : INotification
{
    #region Constructors

    public CheckDownloadQueue(List<int> plexServerIds)
    {
        PlexServerIds = plexServerIds;
    }

    #endregion

    #region Properties

    public List<int> PlexServerIds { get; }

    #endregion
}