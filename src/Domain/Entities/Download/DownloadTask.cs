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
        public PlexMediaType MediaType { get; set; }

        public DownloadStatus DownloadStatus { get; set; }

        public DateTime Created { get; set; }

        /// <summary>
        /// The unique identifier used by the Plex Api to keep track of media.
        /// This is only unique on that specific server.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
        /// </summary>
        public long Priority { get; set; }

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

        public List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = new List<DownloadWorkerTask>();

        #endregion

        #region Helpers

        [NotMapped]
        public long DataTotal => MetaData?.MediaData?.First()?.Parts?.First()?.Size ?? 0;

        [NotMapped]
        public long DataReceived => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.Sum(x => x.BytesReceived) : 0;

        [NotMapped]
        public decimal Percentage => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.Average(x => x.Percentage) : 0;

        /// <summary>
        /// The release year of the media.
        /// </summary>
        [NotMapped]
        public int ReleaseYear => MetaData?.ReleaseYear ?? -1;

        [NotMapped]
        public int MediaParts => DownloadWorkerTasks?.Count ?? 0;

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
        /// The full formatted media title, based on the <see cref="PlexMediaType"/>.
        /// E.g. "TvShow/Season/Episode".
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
        /// </summary>
        [NotMapped]
        public string DestinationFilePath => DestinationFolder != null ? Path.Combine(DestinationFolder.DirectoryPath, MediaPath, FileName) : string.Empty;

        [NotMapped]
        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        [NotMapped]
        public int DownloadSpeed => DownloadWorkerTasks.Any() ? DownloadWorkerTasks.AsQueryable().Sum(x => x.DownloadSpeed) : 0;

        [NotMapped]
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        [NotMapped]
        public long BytesRemaining => DataTotal - DataReceived;

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
                        return Path.Combine($"{Title}" + (ReleaseYear > 0 ? $" ({ReleaseYear})" : string.Empty).SanitizeFolderName());
                    case PlexMediaType.Episode:
                        // If the same, than it will most likely be an anime type of tvShow which can have no seasons.
                        if (TitleTvShow == TitleTvShowSeason)
                        {
                            return Path.Combine(TitleTvShow.SanitizeFolderName());
                        }

                        return Path.Combine(TitleTvShow.SanitizeFolderName(), TitleTvShowSeason.SanitizeFolderName());
                    default:
                        return Path.Combine($"{FileNameWithoutExtention.SanitizeFolderName()}");
                }
            }
        }

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