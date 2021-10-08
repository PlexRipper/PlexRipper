using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShowSeason : PlexMedia
    {
        /// <summary>
        /// The PlexKey of the tvShow this belongs too.
        /// </summary>
        public int ParentKey { get; set; }

        #region Relationships

        public PlexTvShow TvShow { get; set; }

        public int TvShowId { get; set; }

        public List<PlexTvShowEpisode> Episodes { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public override PlexMediaType Type => PlexMediaType.Season;

        #endregion
    }
}