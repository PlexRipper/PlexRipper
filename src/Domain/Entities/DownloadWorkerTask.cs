using System;
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

        private DownloadWorkerTask() { }

        public DownloadWorkerTask(DownloadTask downloadTask)
        {
            FileName = downloadTask.FileName;
            TempDirectory = downloadTask.TempDirectory;
            DownloadTask = downloadTask;
            DownloadTaskId = downloadTask.Id;
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

        /// <summary>
        /// Gets the total bytes received so far.
        /// </summary>
        [Column(Order = 5)]
        public long BytesReceived { get; set; }

        [Column(Order = 6)]
        public string Url { get; set; }

        /// <summary>
        /// The download directory where the part is downloaded into.
        /// </summary>
        [Column(Order = 7)]
        public string TempDirectory { get; internal set; }

        /// <summary>
        /// The elapsed time in milliseconds with an accuracy of 100 milliseconds.
        /// </summary>
        [Column(Order = 8)]
        public long ElapsedTime { get; set; }

        #endregion

        #region Relationships

        public DownloadTask DownloadTask { get; set; }

        public int DownloadTaskId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string TempFileName => $"{Path.GetFileNameWithoutExtension(FileName)}.part{PartIndex}{Path.GetExtension(FileName)}";

        [NotMapped]
        public string TempFilePath => Path.Combine(TempDirectory, TempFileName);

        [NotMapped]
        public Uri Uri => new Uri(Url);

        [NotMapped]
        public long BytesRangeSize => EndByte - StartByte;

        [NotMapped]
        public long BytesRemaining => BytesRangeSize - BytesReceived;

        /// <summary>
        /// The current byte in its range this task is currently downloading at.
        /// </summary>
        [NotMapped]
        public long CurrentByte => StartByte + BytesReceived;

        /// <summary>
        /// Has then <see cref="DownloadWorkerTask"/> been partially completed.
        /// </summary>
        [NotMapped]
        public bool IsResumed => BytesReceived > 0;

        /// <summary>
        /// The elapsed time this <see cref="DownloadWorkerTask"/> has been downloading for.
        /// </summary>
        [NotMapped]
        public TimeSpan ElapsedTimeSpan => TimeSpan.FromMilliseconds(ElapsedTime);

        #endregion
    }
}