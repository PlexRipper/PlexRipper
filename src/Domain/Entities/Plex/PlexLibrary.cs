using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    public class PlexLibrary : BaseEntity
    {
        #region Properties

        /// <summary>
        /// The Library Section Identifier used by Plex
        /// </summary>
        public string Key { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Plex Library type, see: https://github.com/Arcanemagus/plex-api/wiki/MediaTypes
        /// </summary>
        public string Type { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ScannedAt { get; set; }

        public DateTime ContentChangedAt { get; set; }

        public Guid Uuid { get; set; }

        /// <summary>
        /// This is the relative path Id of the Library location
        /// </summary>
        public int LibraryLocationId { get; set; }

        /// <summary>
        /// This is a relative path of the Library location, e.g: /AnimeSeries
        /// </summary>
        public string LibraryLocationPath { get; set; }

        #endregion

        #region Relationships

        /// <summary>
        /// The PlexServer this PlexLibrary belongs to
        /// </summary>
        public PlexServer PlexServer { get; set; }

        /// <summary>
        /// The PlexServerId of the PlexServer this PlexLibrary belongs to
        /// </summary>
        public int PlexServerId { get; set; }

        public List<PlexMovie> Movies { get; set; }

        public List<PlexTvShow> TvShows { get; set; }

        public List<PlexAccountLibrary> PlexAccountLibraries { get; set; }

        public List<DownloadTask> DownloadTasks { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public PlexMediaType MediaType
        {
            get
            {
                return Type switch
                {
                    "movie" => PlexMediaType.Movie,
                    "show" => PlexMediaType.TvShow,
                    "artist" => PlexMediaType.Music,
                    _ => PlexMediaType.Unknown
                };
            }

            set
            {
                switch (value)
                {
                    case PlexMediaType.None:
                        Type = "none";
                        return;
                    case PlexMediaType.Movie:
                        Type = "movie";
                        return;
                    case PlexMediaType.TvShow:
                        Type = "show";
                        return;
                    case PlexMediaType.Season:
                        Type = "show";
                        return;
                    case PlexMediaType.Episode:
                        Type = "episode";
                        return;
                    case PlexMediaType.Music:
                        Type = "music";
                        return;
                    case PlexMediaType.Album:
                        Type = "album";
                        return;
                    case PlexMediaType.Unknown:
                        Type = "unknown";
                        return;
                }
            }
        }

        [NotMapped]
        public bool HasMedia
        {
            get
            {
                return MediaType switch
                {
                    PlexMediaType.Movie => Movies != null && Movies.Count > 0,
                    PlexMediaType.TvShow => TvShows != null && TvShows.Count > 0,
                    _ => false,
                };
            }
        }

        [NotMapped]
        public int GetMediaCount
        {
            get
            {
                return MediaType switch
                {
                    PlexMediaType.Movie => Movies?.Count ?? -1,
                    PlexMediaType.TvShow => TvShows?.Count ?? -1,
                    _ => -1,
                };
            }
        }

        [NotMapped]
        public string Name => Title;

        #endregion
    }
}