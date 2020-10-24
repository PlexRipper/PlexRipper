using System;
using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexTvShowSeason : BaseEntity
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

        public DateTime AddedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime OriginallyAvailableAt { get; set; }


        #region Helpers

        public PlexMediaType Type => PlexMediaType.Season;

        #endregion

        #region Relationships

        public PlexTvShow TvShow { get; set; }

        public int TvShowId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        public List<PlexTvShowEpisode> Episodes { get; set; }

        #endregion
    }
}