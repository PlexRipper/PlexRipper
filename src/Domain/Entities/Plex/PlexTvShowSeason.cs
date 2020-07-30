using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShowSeason : PlexMedia
    {
        #region Relationships
        public PlexTvShow TvShow { get; set; }
        #endregion
    }
}
