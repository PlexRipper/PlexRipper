using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities
{
    public class DownloadTask : BaseEntity
    {
        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv
        /// </summary>
        public string FileLocationUrl { get; set; }
        public string FileName { get; set; }

        public string DownloadStatus { get; set; }

        #region Relationships
        public virtual PlexServer PlexServer { get; set; }

        public int PlexServerId { get; set; }

        public virtual FolderPath FolderPath { get; set; }
        public int FolderPathId { get; set; }
        #endregion

        #region Helpers
        [NotMapped]
        public DownloadStatus Status
        {
            get => Enum.TryParse(DownloadStatus, out DownloadStatus status) ? status : Enums.DownloadStatus.Unknown;
            set => DownloadStatus = value.ToString();
        }

        [NotMapped]
        public Uri DownloadUri => new Uri(DownloadUrl, UriKind.Absolute);

        [NotMapped]
        public string DownloadUrl =>
            $"{PlexServer.BaseUrl}{FileLocationUrl}?download=1&X-Plex-Token={PlexServer.AccessToken}";

        #endregion
    }
}
