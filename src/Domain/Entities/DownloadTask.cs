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

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// If this type is an episode of a tv show then this will be the title of that tv show.
        /// </summary>
        public string TitleTvShow { get; set; }

        /// <summary>
        /// If this type is an episode of a tv show then this will be the title of that tv show season.
        /// </summary>
        public string TitleTvShowSeason { get; set; }

        [Column("Type")]
        public string _Type { get; set; }

        [Column("DownloadStatus")]
        public string _DownloadStatus { get; set; }

        #region Relationships

        public virtual PlexServer PlexServer { get; set; }
        public int PlexServerId { get; set; }

        public virtual FolderPath FolderPath { get; set; }
        public int FolderPathId { get; set; }

        #endregion

        #region Helpers
        [NotMapped] //TODO Debate if the token should be stored in the database or retrieved
        public string PlexServerAuthToken { get; set; }

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
        public string DownloadUrl =>
            $"{PlexServer.BaseUrl}{FileLocationUrl}?download=1&X-Plex-Token={PlexServerAuthToken}";

        [NotMapped]
        public string DownloadDirectory { get; set; }

        #endregion
    }
}
