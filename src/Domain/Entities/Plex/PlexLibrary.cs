using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexLibrary : BaseEntity
    {
        #region Properties

        /// <summary>
        /// The Library Section Identifier used by Plex.
        /// </summary>
        [Column(Order = 1)]
        public string Key { get; set; }

        [Column(Order = 2)]
        public string Title { get; set; }

        /// <summary>
        /// Plex Library type, see: https://github.com/Arcanemagus/plex-api/wiki/MediaTypes.
        /// </summary>
        [Column(Order = 3)]
        public PlexMediaType Type { get; set; }

        [Column(Order = 4)]
        public DateTime CreatedAt { get; set; }

        [Column(Order = 5)]
        public DateTime UpdatedAt { get; set; }

        [Column(Order = 6)]
        public DateTime ScannedAt { get; set; }

        [Column(Order = 7)]
        public DateTime ContentChangedAt { get; set; }

        /// <summary>
        /// The DateTime this library was last refreshed from the PlexApi.
        /// </summary>
        [Column(Order = 8)]
        public DateTime CheckedAt { get; set; }

        [Column(Order = 9)]
        public Guid Uuid { get; set; }

        /// <summary>
        /// The total filesize of the nested media.
        /// </summary>
        [Column(Order = 10)]
        public long MediaSize { get; set; }

        /// <summary>
        /// This is the relative path Id of the Library location.
        /// </summary>
        [Column(Order = 10)]
        public int LibraryLocationId { get; set; }

        /// <summary>
        /// This is a relative path of the Library location, e.g: /AnimeSeries.
        /// </summary>
        [Column(Order = 11)]
        public string LibraryLocationPath { get; set; }

        #endregion

        #region Relationships

        /// <summary>
        /// The PlexServer this PlexLibrary belongs to.
        /// </summary>
        public PlexServer PlexServer { get; set; }

        /// <summary>
        /// The PlexServerId of the PlexServer this PlexLibrary belongs to.
        /// </summary>
        public int PlexServerId { get; set; }

        public List<PlexMovie> Movies { get; set; }

        public List<PlexTvShow> TvShows { get; set; }

        public List<PlexAccountLibrary> PlexAccountLibraries { get; set; }

        public List<DownloadTask> DownloadTasks { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public bool HasMedia
        {
            get
            {
                return Type switch
                {
                    PlexMediaType.Movie => Movies != null && Movies.Count > 0,
                    PlexMediaType.TvShow => TvShows != null && TvShows.Count > 0,
                    _ => false,
                };
            }
        }

        [NotMapped]
        public int MediaCount
        {
            get
            {
                return Type switch
                {
                    PlexMediaType.Movie => Movies?.Count ?? -1,
                    PlexMediaType.TvShow => TvShows?.Count ?? -1,
                    _ => -1,
                };
            }
        }

        [NotMapped]
        public int SeasonCount
        {
            get
            {
                return Type switch
                {
                    PlexMediaType.Movie => 0,
                    PlexMediaType.TvShow => TvShows?.Sum(x => x.Seasons.Count) ?? 0,
                    _ => -1,
                };
            }
        }

        [NotMapped]
        public int EpisodeCount
        {
            get
            {
                return Type switch
                {
                    PlexMediaType.Movie => 0,
                    PlexMediaType.TvShow => TvShows?.Sum(x => x.Seasons.Sum(y => y.Episodes.Count)) ?? 0,
                    _ => -1,
                };
            }
        }

        [NotMapped]
        public string ServerUrl => PlexServer?.ServerUrl ?? string.Empty;

        [NotMapped]
        public string Name => Title;

        public void ClearMedia()
        {
            // TODO Add here other types
            Movies = null;
            TvShows = null;
        }

        /// <summary>
        /// Sort the containing media.
        /// </summary>
        /// <returns>The <see cref="PlexLibrary"/> with its media sorted.</returns>
        public PlexLibrary SortMedia()
        {
            // Sort Movies
            if (Movies?.Count > 0)
            {
                Movies = Movies.OrderByNatural(x => x.Title).ToList();
            }

            // Sort TvShows
            if (TvShows?.Count > 0)
            {
                TvShows = TvShows.OrderBy(x => x.Title).ThenBy(y => y.RatingKey).ToList();
                for (int i = 0; i < TvShows.Count; i++)
                {
                    TvShows[i].Seasons = TvShows[i].Seasons.OrderByNatural(x => x.Title).ToList();

                    for (int j = 0; j < TvShows[i].Seasons.Count; j++)
                    {
                        TvShows[i].Seasons[j].Episodes =
                            TvShows[i].Seasons[j].Episodes.OrderBy(x => x.RatingKey).ToList();
                    }
                }
            }

            // TODO Add here for other media types once supported
            return this;
        }

        #endregion
    }
}