using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using FluentResults;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Creates a media DownloadTask to be executed, needed values should be copied over since the mediaIds can change randomly.
    /// </summary>
    public class DownloadTask : BaseEntity
    {
        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
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

        /// <summary>
        /// The release year of the media.
        /// </summary>
        public int ReleaseYear { get; set; }

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

        /// <summary>
        /// Used to authenticate download request with the server.
        /// </summary>
        public string ServerToken { get; set; }

        #region Relationships

        public PlexServer PlexServer { get; set; }

        public int PlexServerId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

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
        public string DownloadUrl => PlexServer != null ? $"{PlexServer?.ServerUrl}{FileLocationUrl}?X-Plex-Token={ServerToken}" : string.Empty;

        [NotMapped]
        public Uri DownloadUri => new Uri(DownloadUrl, UriKind.Absolute);

        /// <summary>
        /// Gets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
        /// </summary>
        [NotMapped]
        public string DownloadPath => Path.Combine(DownloadFolder.DirectoryPath, MediaPath);

        /// <summary>
        /// Gets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
        /// </summary>
        [NotMapped]
        public string DestinationPath => Path.Combine(DestinationFolder.DirectoryPath, MediaPath);

        /// <summary>
        /// Gets the sub-path based on the <see cref="PlexMediaType"/>, e.g: [TvShow]/[Season]/ or [Movie] [ReleaseYear]/.
        /// </summary>
        [NotMapped]
        public string MediaPath
        {
            get
            {
                switch (MediaType)
                {
                    case PlexMediaType.Movie:
                        return $"{Title} " + (ReleaseYear > 0 ? $"({ReleaseYear})" : string.Empty);
                    case PlexMediaType.Episode:
                        return Path.Combine(TitleTvShow.Replace(":", "-"), TitleTvShowSeason);
                    default:
                        return Path.Combine($"{Path.GetFileNameWithoutExtension(FileName)}");
                }
            }
        }

        public Result IsValid()
        {
            return new AddDownloadTaskValidator().Validate(this).ToFluentResult();
        }

        #endregion
    }
}