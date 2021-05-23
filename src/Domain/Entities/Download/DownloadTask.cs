using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using FluentResults;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Creates a media DownloadTask to be executed, needed values should be copied over since the mediaIds can change randomly.
    /// </summary>
    public class DownloadTask : BaseEntity
    {
        public PlexMediaType MediaType { get; set; }

        public DownloadStatus DownloadStatus { get; set; }

        public DateTime Created { get; set; }

        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
        /// </summary>
        public long Priority { get; set; }

        public long DataReceived { get; set; }

        /// <summary>
        /// Used to authenticate download request with the server.
        /// </summary>
        public string ServerToken { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DownloadTaskMetaData"/>, this is a JSON field that contains a collection
        /// of various values that dont warrant their own database column.
        /// </summary>
        public DownloadTaskMetaData MetaData { get; set; }

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
        public long DataTotal => MetaData?.MediaData?.First()?.Parts?.First()?.Size ?? 0;

        [NotMapped]
        public decimal Percentage => decimal.Round(DataReceived / DataTotal, 2);

        /// <summary>
        /// The release year of the media.
        /// </summary>
        [NotMapped]
        public int ReleaseYear => MetaData.ReleaseYear;

        [NotMapped]
        public int MediaParts => MetaData.MediaData.Count;

        /// <summary>
        /// The formatted media title as shown in Plex, based on the <see cref="PlexMediaType"/>.
        /// </summary>
        [NotMapped]
        public string Title
        {
            get
            {
                return MediaType switch
                {
                    PlexMediaType.Movie => TitleMovie,
                    PlexMediaType.TvShow => TitleTvShow,
                    PlexMediaType.Season => TitleTvShowSeason,
                    PlexMediaType.Episode => TitleTvShowEpisode,
                    _ => "TitleNotFound",
                };
            }
        }

        /// <summary>
        /// The formatted media title as shown in Plex, based on the <see cref="PlexMediaType"/>.
        /// </summary>
        [NotMapped]
        public string TitlePath
        {
            get
            {
                return MediaType switch
                {
                    PlexMediaType.Movie => TitleMovie,
                    PlexMediaType.TvShow => TitleTvShow,
                    PlexMediaType.Season => $"{TitleTvShow}/{TitleTvShowSeason}",
                    PlexMediaType.Episode => $"{TitleTvShow}/{TitleTvShowSeason}/{TitleTvShowEpisode}",
                    _ => "TitleNotFound",
                };
            }
        }

        [NotMapped]
        public string TitleMovie => MetaData?.MovieTitle ?? string.Empty;

        /// <summary>
        /// If the type is an TvShow, Season or Episode, then this will be the title of that TvShow.
        /// </summary>
        [NotMapped]
        public string TitleTvShow => MetaData?.TvShowTitle ?? string.Empty;

        /// <summary>
        /// If the type is an TvShow, Season or Episode, then this will be the title of that TvShow season.
        /// </summary>
        [NotMapped]
        public string TitleTvShowSeason => MetaData?.TvShowSeasonTitle ?? string.Empty;

        /// <summary>
        /// If the type is an TvShow, Season or Episode, then this will be the title of that TvShow episode.
        /// </summary>
        [NotMapped]
        public string TitleTvShowEpisode => MetaData?.TvShowEpisodeTitle ?? string.Empty;

        [NotMapped]
        public string FileName => Path.GetFileName(MetaData?.MediaData?.First()?.Parts?.First()?.File ?? string.Empty);

        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
        /// </summary>
        [NotMapped]
        public string FileLocationUrl => MetaData?.MediaData?.First()?.Parts?.First()?.ObfuscatedFilePath ?? string.Empty;

        [NotMapped]
        public string DownloadUrl => PlexServer != null ? $"{PlexServer?.ServerUrl}{FileLocationUrl}?X-Plex-Token={ServerToken}" : string.Empty;

        [NotMapped]
        public Uri DownloadUri => !string.IsNullOrWhiteSpace(DownloadUrl) ? new Uri(DownloadUrl, UriKind.Absolute) : null;

        [NotMapped]
        public string FileNameWithoutExtention => Path.GetFileNameWithoutExtension(FileName);

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
                        return Path.Combine(DownloadFolder.DirectoryPath, "Movies", $"{FileNameWithoutExtention}".SanitizePath());
                    case PlexMediaType.Episode:
                        return Path.Combine(DownloadFolder.DirectoryPath, "TvShows", $"{FileNameWithoutExtention}".SanitizePath());
                    default:
                        return Path.Combine(DownloadFolder.DirectoryPath, "Other", $"{FileNameWithoutExtention}".SanitizePath());
                }
            }
        }

        /// <summary>
        /// Gets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
        /// </summary>
        [NotMapped]
        public string DestinationPath => Path.Combine(DestinationFolder.DirectoryPath, MediaPath);

        /// <summary>
        /// Gets the sanitized sub-path based on the <see cref="PlexMediaType"/>, e.g: [TvShow]/[Season]/ or [Movie] [ReleaseYear]/.
        /// </summary>
        [NotMapped]
        public string MediaPath
        {
            get
            {
                switch (MediaType)
                {
                    case PlexMediaType.Movie:
                        return Path.Combine($"{Title}" + (ReleaseYear > 0 ? $" ({ReleaseYear})" : string.Empty).SanitizePath());
                    case PlexMediaType.Episode:
                        // If the same, than it will most likely be an anime type of tvShow which can have no seasons.
                        if (TitleTvShow == TitleTvShowSeason)
                        {
                            return Path.Combine(TitleTvShow.SanitizePath());
                        }

                        return Path.Combine(TitleTvShow.SanitizePath(), TitleTvShowSeason.SanitizePath());
                    default:
                        return Path.Combine($"{FileNameWithoutExtention.SanitizePath()}");
                }
            }
        }

        public Result IsValid()
        {
            return new DownloadTaskValidator().Validate(this).ToFluentResult();
        }

        #endregion
    }
}