using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using FluentResults;

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
        public long DataTotal { get; set; }

        [Column(Order = 5)]
        public PlexMediaType MediaType { get; set; }

        [Column(Order = 6)]
        public DownloadStatus DownloadStatus { get; set; }

        [Column(Order = 7)]
        public DownloadTaskType DownloadTaskType { get; set; }

        [Column(Order = 8)]
        public DateTime Created { get; set; }

        [Column(Order = 9)]
        public string FileName { get; set; }

        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
        /// </summary>
        [Column(Order = 10)]
        public string FileLocationUrl { get; set; }

        /// <summary>
        /// Gets or sets the full formatted media title, based on the <see cref="PlexMediaType"/>.
        /// E.g. "TvShow/Season/Episode".
        /// </summary>
        [Column(Order = 11)]
        public string FullTitle { get; set; }

        [Column(Order = 12)]
        public string Quality { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
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

        public int ParentId { get; set; }

        public DownloadTask Parent { get; set; }

        public List<DownloadTask> Children { get; set; }

        public int MediaId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public long DataReceived => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.Sum(x => x.BytesReceived) : 0;

        [NotMapped]
        public decimal Percentage => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.Average(x => x.Percentage) : 0;

        [NotMapped]
        public int MediaParts => DownloadWorkerTasks?.Count ?? 0;

        // TODO Add Server Token
        [NotMapped]
        public string DownloadUrl => PlexServer != null ? $"{PlexServer?.ServerUrl}{FileLocationUrl}" : string.Empty;

        [NotMapped]
        public Uri DownloadUri => !string.IsNullOrWhiteSpace(DownloadUrl) ? new Uri(DownloadUrl, UriKind.Absolute) : null;

        [NotMapped]
        public string FileNameWithoutExtention => !string.IsNullOrWhiteSpace(FileName) ? Path.GetFileNameWithoutExtension(FileName) : string.Empty;

        /// <summary>
        /// Gets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
        /// </summary>
        [NotMapped]
        public string DownloadPath
        {
            get
            {
                if (DownloadFolder is null)
                {
                    return string.Empty;
                }

                switch (MediaType)
                {
                    case PlexMediaType.Movie:
                        return Path.Combine(DownloadFolder.DirectoryPath, "Movies", $"{FileNameWithoutExtention}".SanitizeFolderName());
                    case PlexMediaType.Episode:
                        return Path.Combine(DownloadFolder.DirectoryPath, "TvShows", $"{FileNameWithoutExtention}".SanitizeFolderName());
                    default:
                        return Path.Combine(DownloadFolder.DirectoryPath, "Other", $"{FileNameWithoutExtention}".SanitizeFolderName());
                }
            }
        }

        /// <summary>
        /// Gets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
        /// TODO Get valid destination file path.
        /// </summary>
        [NotMapped]
        public string DestinationFilePath
        {
            get
            {
                if (DestinationFolder != null)
                {
                    switch (MediaType)
                    {
                        case PlexMediaType.Movie:
                            return Path.Combine(DestinationFolder.DirectoryPath, FullTitle, FileName);
                        case PlexMediaType.TvShow:
                            break;
                        case PlexMediaType.Season:
                            break;
                        case PlexMediaType.Episode:
                            break;
                        case PlexMediaType.Music:
                            break;
                        case PlexMediaType.Album:
                            break;
                        case PlexMediaType.Song:
                            break;
                        case PlexMediaType.Photos:
                            break;
                        case PlexMediaType.OtherVideos:
                            break;
                        case PlexMediaType.Games:
                            break;
                        case PlexMediaType.Unknown:
                            break;
                        default:
                            return Path.Combine(DestinationFolder.DirectoryPath, FullTitle, FileName);
                    }
                }

                return "INVALID";
            }
        }

        [NotMapped]
        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        [NotMapped]
        public int DownloadSpeed => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.AsQueryable().Sum(x => x.DownloadSpeed) : 0;

        [NotMapped]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        [NotMapped]
        public long BytesRemaining => DataTotal - DataReceived;

        public Result IsValid()
        {
            return new DownloadTaskValidator().Validate(this).ToFluentResult();
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
    }
}