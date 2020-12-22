using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShowSeason : BaseEntity, IToDownloadTask
    {
        /// <summary>
        /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
        /// e.g: 28550, 1723, 21898.
        /// </summary>
        public int RatingKey { get; set; }

        /// <summary>
        /// Unique key identifying this item by the Plex Api. with the url path included
        /// e.g: "/library/metadata/9725", "/library/metadata/9724".
        /// TODO: This can possibly be removed from the database as it seems that every "Key" is always "/library/metadata/" + RatingKey.
        /// </summary>
        public string Key { get; set; }

        public string Guid { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public int Index { get; set; }

        public int LeafCount { get; set; }

        public int ViewedLeafCount { get; set; }

        public int ChildCount { get; set; }

        /// <summary>
        /// The total filesize of the nested media.
        /// </summary>
        public long MediaSize { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime OriginallyAvailableAt { get; set; }

        #region Relationships

        public PlexTvShow TvShow { get; set; }

        public int TvShowId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        public List<PlexTvShowEpisode> Episodes { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public PlexMediaType Type => PlexMediaType.Season;

        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTasks = Episodes.SelectMany(x => x.CreateDownloadTasks()).ToList();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.TitleTvShowSeason = Title;
            }

            return downloadTasks;
        }

        #endregion
    }
}