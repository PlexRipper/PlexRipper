using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
/// This holds the task and state of individual DownloadWorkers.
/// </summary>
public class DownloadWorkerTask : BaseEntity
{
    #region Properties

    /// <summary>
    /// The base filename of the media that will be downloaded.
    /// </summary>
    [Column(Order = 1)]
    public required string FileName { get; init; }

    [Column(Order = 2)]
    public required int PartIndex { get; init; }

    [Column(Order = 3)]
    public required long StartByte { get; init; }

    [Column(Order = 4)]
    public required long EndByte { get; init; }

    [Column(Order = 5)]
    public required DownloadStatus DownloadStatus { get; set; } = DownloadStatus.Queued;

    /// <summary>
    /// Gets the total bytes received so far.
    /// </summary>
    [Column(Order = 7)]
    public required long BytesReceived { get; set; }

    /// <summary>
    /// The download directory where the part is downloaded into.
    /// </summary>
    [Column(Order = 8)]
    public required string DownloadDirectory { get; init; }

    /// <summary>
    /// The elapsed time in milliseconds with an accuracy of 100 milliseconds.
    /// </summary>
    [Column(Order = 9)]
    public required long ElapsedTime { get; set; }

    [Column(Order = 10)]
    public required string FileLocationUrl { get; init; }

    #endregion

    #region Relationships

    public DownloadTaskFileBase? DownloadTask { get; set; }
    public required Guid DownloadTaskId { get; init; }

    public PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; init; }

    public ICollection<DownloadWorkerLog> DownloadWorkerTaskLogs { get; init; } = [];

    #endregion

    #region Helpers

    [NotMapped]
    public string DownloadFilePath => Path.Combine(DownloadDirectory, FileName);

    [NotMapped]
    public long DataTotal => EndByte - StartByte;

    [NotMapped]
    public long DataRemaining => DataTotal - BytesReceived;

    /// <summary>
    /// The current byte in its range this task is currently downloading at.
    /// </summary>
    [NotMapped]
    public long CurrentByte => StartByte + BytesReceived;

    /// <summary>
    /// The elapsed time this <see cref="DownloadWorkerTask"/> has been downloading for.
    /// </summary>
    [NotMapped]
    public TimeSpan ElapsedTimeSpan => TimeSpan.FromMilliseconds(ElapsedTime);

    [NotMapped]
    public int DownloadSpeed => DataFormat.GetTransferSpeed(BytesReceived, ElapsedTimeSpan.TotalSeconds);

    [NotMapped]
    public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

    /// <summary>
    /// The time remaining in seconds for this DownloadWorker to finish.
    /// </summary>
    [NotMapped]
    public long TimeRemaining => DataFormat.GetTimeRemaining(DataRemaining, DownloadSpeed);

    /// <summary>
    /// The time elapsed of this DownloadWorker.
    /// </summary>
    [NotMapped]
    public bool IsCompleted => BytesReceived == DataTotal;

    [NotMapped]
    public decimal Percentage => DataFormat.GetPercentage(BytesReceived, DataTotal);

    #endregion
}
