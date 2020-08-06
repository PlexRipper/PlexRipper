using System;
using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShowEpisode : BaseEntity
    {
       
        public int RatingKey { get; set; }
        public string Key { get; set; }
        public string Guid { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Index { get; set; }
        public string Type { get; set; }
        public int LeafCount { get; set; }
        public int ViewedLeafCount { get; set; }
        public int ChildCount { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime OriginallyAvailableAt { get; set; }


        #region Helpers


        #endregion
        
        #region Relationships
        public PlexTvShowSeason TvShowSeason { get; set; }
        public int TvShowSeasonId { get; set; }
        #endregion
    }
}
