using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PlexRipper.Domain
{
    /// <summary>
    /// This holds the task and state of individual DownloadWorkers.
    /// </summary>
    public class DownloadWorkerTask : BaseEntity
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadWorkerTask"/> class.
        /// </summary>
        /// <param name="downloadTask"></param>
        /// <param name="partIndex"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        public DownloadWorkerTask(DownloadTask downloadTask, int partIndex, long startPosition, long endPosition)
        {
            TempDirectory = downloadTask.DownloadDirectory;
            DownloadUrl = downloadTask.DownloadUrl;
            DownloadTaskId = downloadTask.Id;
            PartIndex = partIndex;
            StartByte = startPosition;
            EndByte = endPosition;
            FileName = $"{Path.GetFileNameWithoutExtension(downloadTask.FileName)}.part{partIndex}{Path.GetExtension(downloadTask.FileName)}";
        }

        public DownloadWorkerTask()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// The base filename of the media that will be downloaded.
        /// </summary>
        [Column(Order = 1)]
        public string FileName { get; internal set; }

        [Column(Order = 2)]
        public int PartIndex { get; set; }

        [Column(Order = 3)]
        public long StartByte { get; set; }

        [Column(Order = 4)]
        public long EndByte { get; set; }

        [Column(Order = 5)]
        public DownloadStatus DownloadStatus { get; set; } = DownloadStatus.Initialized;

        /// <summary>
        /// Gets the total bytes received so far.
        /// </summary>
        [Column(Order = 7)]
        public long BytesReceived { get; set; }

        /// <summary>
        /// The download directory where the part is downloaded into.
        /// </summary>
        [Column(Order = 8)]
        public string TempDirectory { get; internal set; }

        /// <summary>
        /// The elapsed time in milliseconds with an accuracy of 100 milliseconds.
        /// </summary>
        [Column(Order = 9)]
        public long ElapsedTime { get; set; }

        [Column(Order = 10)]
        public string DownloadUrl { get; set; }

        #endregion

        #region Relationships

        public DownloadTask DownloadTask { get; set; }

        public int DownloadTaskId { get; set; }

        public ICollection<DownloadWorkerLog> DownloadWorkerTaskLogs { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string TempFilePath => Path.Combine(TempDirectory, FileName);

        [NotMapped]
        public Uri Uri => new(DownloadUrl);

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
}