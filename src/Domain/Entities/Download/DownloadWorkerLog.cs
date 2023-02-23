using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class DownloadWorkerLog : BaseEntity
{
    #region Properties

    [Column(Order = 1)]
    public DateTime CreatedAt { get; set; }

    [Column(Order = 3)]
    public NotificationLevel LogLevel { get; set; }

    [Column(Order = 2)]
    public string Message { get; set; }

    #endregion

    #region Relationships

    public DownloadWorkerTask DownloadTask { get; set; }

    public int DownloadWorkerTaskId { get; set; }

    #endregion
}