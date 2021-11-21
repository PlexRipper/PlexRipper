using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Creates a media DownloadTask to be executed and is also used for providing updates on progress.
    /// Needed values from <see cref="PlexMedia"/> should be copied over since the mediaIds can change randomly.
    /// </summary>
    public class DownloadTask : BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier used by the Plex Api to keep track of media.
        /// This is only unique on that specific server.
        /// </summary>
        [Column(Order = 1)]
        public int Key { get; set; }

        /// <summary>
        /// Gets or sets the media display title.
        /// </summary>
        [Column(Order = 2)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the release year of the media.
        /// </summary>
        [Column(Order = 3)]
        public int Year { get; set; }

        [Column(Order = 4)]
        public decimal Percentage { get; set; }

        [Column(Order = 5)]
        public long DataReceived { get; set; }

        [Column(Order = 6)]
        public long DataTotal { get; set; }

        [Column(Order = 7)]
        public PlexMediaType MediaType { get; set; }

        [Column(Order = 8)]
        public DownloadStatus DownloadStatus { get; set; }

        [Column(Order = 9)]
        public DownloadTaskType DownloadTaskType { get; set; }

        [Column(Order = 10)]
        public DateTime Created { get; set; }

        [Column(Order = 11)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the relative obfuscated URL of the media to be downloaded,
        /// e.g: /library/parts/47660/156234666/file.mkv.
        /// </summary>
        [Column(Order = 12)]
        public string FileLocationUrl { get; set; }

        /// <summary>
        /// Gets or sets the full download url including the <see cref="PlexServer"/> token of the media to be downloaded.
        /// This is only set if <see cref="DownloadTask"/> is downloadable.
        /// </summary>
        [Column(Order = 13)]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the full formatted media title, based on the <see cref="PlexMediaType"/>.
        /// E.g. "TvShow/Season/Episode".
        /// </summary>
        [Column(Order = 14)]
        public string FullTitle { get; set; }

        /// <summary>
        /// Gets or sets get or sets the media quality of this <see cref="DownloadTask"/>.
        /// </summary>
        [Column(Order = 15)]
        public string Quality { get; set; }

        /// <summary>
        /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
        /// </summary>
        [Column(Order = 16)]
        public string DownloadDirectory { get; set; }

        /// <summary>
        /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
        /// </summary>
        [Column(Order = 17)]
        public string DestinationDirectory { get; set; }

        [Column(Order = 18)]
        public int DownloadSpeed { get; set; }

        /// <summary>
        /// Gets or sets the download priority, the higher the more important.
        /// </summary>
        public long Priority { get; set; }

        #region Relationships

        public PlexServer PlexServer { get; set; }

        public int PlexServerId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        public FolderPath DestinationFolder { get; set; }

        public int DestinationFolderId { get; set; }

        public FolderPath DownloadFolder { get; set; }

        public int DownloadFolderId { get; set; }

        public List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = new List<DownloadWorkerTask>();

        public int? ParentId { get; set; }

        public DownloadTask Parent { get; set; }

        public List<DownloadTask> Children { get; set; }

        public int MediaId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public int MediaParts => DownloadWorkerTasks?.Count ?? 0;

        [NotMapped]
        public Uri DownloadUri => !string.IsNullOrWhiteSpace(DownloadUrl) ? new Uri(DownloadUrl, UriKind.Absolute) : null;

        [NotMapped]
        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        [NotMapped]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        [NotMapped]
        public long BytesRemaining => DataTotal - DataReceived;

        /// <summary>
        /// Gets a joined string of temp file paths of the <see cref="DownloadWorkerTasks"/> delimited by ";".
        /// </summary>
        [NotMapped]
        public string GetFilePathsCompressed => string.Join(';', DownloadWorkerTasks.Select(x => x.TempFilePath).ToArray());

        /// <summary>
        /// Gets a value indicating whether this <see cref="DownloadTask"/> is downloadable.
        /// e.g. A episode or movie part, an episode or movie without parts.
        /// </summary>
        public bool IsDownloadable =>
            IsDownloadTaskPart()
            || DownloadTaskType is DownloadTaskType.Episode && !Children.Any()
            || DownloadTaskType is DownloadTaskType.Movie && !Children.Any();

        public bool IsDownloadTaskPart()
        {
            return DownloadTaskType
                is DownloadTaskType.EpisodePart
                or DownloadTaskType.MoviePart;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var orderedList = DownloadWorkerTasks?.OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            builder.Append($"[Status: {DownloadStatus}] - ");
            foreach (var progress in orderedList)
            {
                builder.Append($"({progress.Id} - {progress.Percentage} {progress.DownloadSpeedFormatted}) + ");
            }

            // Remove the last " + "
            if (builder.Length > 3)
            {
                builder.Length -= 3;
            }

            builder.Append($" = ({DownloadSpeedFormatted} - {TimeRemaining})");

            return builder.ToString();
        }

        #endregion

        #region Equality Members

        public bool Equals(DownloadTask other)
        {
            if (other is null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((DownloadTask)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}