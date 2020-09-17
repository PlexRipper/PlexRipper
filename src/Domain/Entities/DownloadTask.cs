using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PlexRipper.Domain
{
    public class DownloadTask : BaseEntity
    {
        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv
        /// </summary>
        public string FileLocationUrl { get; set; }

        public string FileName { get; set; }

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// If the type is an episode of a tv show then this will be the title of that tv show.
        /// </summary>
        public string TitleTvShow { get; set; }

        /// <summary>
        /// If the type is an episode of a tv show then this will be the title of that tv show season.
        /// </summary>
        public string TitleTvShowSeason { get; set; }

        [Column("Type")]
        public string _Type { get; set; }

        [Column("DownloadStatus")]
        public string _DownloadStatus { get; set; }

        public DateTime Created { get; set; }

        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        public int RatingKey { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
        /// </summary>
        public long Priority { get; set; }

        public long DataReceived { get; set; }

        public long DataTotal { get; set; }

        #region Relationships

        public PlexServer PlexServer { get; set; }
        public int PlexServerId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        public PlexAccount PlexAccount { get; set; }
        public int PlexAccountId { get; set; }

        public FolderPath DestinationFolder { get; set; }
        public int DestinationFolderId { get; set; }

        public FolderPath DownloadFolder { get; set; }
        public int DownloadFolderId { get; set; }

        public List<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public DownloadStatus DownloadStatus
        {
            get => Enum.TryParse(_DownloadStatus, out DownloadStatus status) ? status : DownloadStatus.Unknown;
            set => _DownloadStatus = value.ToString();
        }

        [NotMapped]
        public PlexMediaType MediaType
        {
            get => Enum.TryParse(_Type, out PlexMediaType type) ? type : PlexMediaType.Unknown;
            set => _Type = value.ToString();
        }

        [NotMapped]
        public Uri DownloadUri => new Uri(DownloadUrl, UriKind.Absolute);

        [NotMapped]
        public string DownloadUrl => $"{PlexServer?.BaseUrl}{FileLocationUrl}" ?? "";

        [NotMapped]
        public string DownloadPath => DownloadFolder?.DirectoryPath ?? "";

        /// <summary>
        /// The download directory with a folder named after the filename
        /// </summary>
        [NotMapped]
        public string TempDirectory
        {
            get
            {
                switch (MediaType)
                {
                    case PlexMediaType.Movie:
                        return Path.Combine(DownloadPath, $"{Path.GetFileNameWithoutExtension(FileName)}");
                    case PlexMediaType.Episode:
                        return Path.Combine(DownloadPath, TitleTvShow.Replace(":", "-"), TitleTvShowSeason);
                    default:
                        return Path.Combine(DownloadPath, $"{Path.GetFileNameWithoutExtension(FileName)}");
                }
            }
        }

        public bool CheckDownloadTask()
        {
            return DownloadFolder.IsValid()
                   && DestinationFolder.IsValid()
                   && !string.IsNullOrEmpty(DownloadUrl);
        }

        #endregion
    }
}