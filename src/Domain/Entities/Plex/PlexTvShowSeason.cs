using System;
using System.Collections.Generic;
using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShowSeason : BaseEntity
    {

        public int RatingKey { get; set; }
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

        public List<PlexTvShowEpisode> Episodes { get; set; }
        #endregion
    }
}
