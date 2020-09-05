using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities
{
    /// <summary>
    /// This holds the task and state of individual DownloadWorkers.
    /// </summary>
    public class DownloadWorkerTask : BaseEntity
    {
        #region Constructors

        public DownloadWorkerTask() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the total bytes received so far.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// The base filename of the media that will be downloaded.
        /// </summary>
        public string FileName { get; set; }

        public long EndByte { get; set; }

        public int PartIndex { get; set; }

        public long StartByte { get; set; }

        public string Url { get; set; }

        public string DownloadDirectory { get; set; }

        /// <summary>
        /// The elapsed time in milliseconds with an accuracy of 100 milliseconds.
        /// </summary>
        public long ElapsedTime { get; set; }

        #endregion

        #region Relationships

        public DownloadTask DownloadTask { get; set; }
        public int DownloadTaskId { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// The download directory with a folder named after the filename
        /// </summary>
        [NotMapped]
        public string TempDirectory =>
            Path.Combine(DownloadDirectory, $"{Path.GetFileNameWithoutExtension(FileName)}");

        [NotMapped]
        public string TempFileName => $"{PartIndex}-{FileName}";

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