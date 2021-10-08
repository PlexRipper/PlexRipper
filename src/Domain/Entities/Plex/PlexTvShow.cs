using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShow : PlexMedia
    {
        public override PlexMediaType Type => PlexMediaType.TvShow;

        #region Relationships

        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }

        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }

        public List<PlexTvShowSeason> Seasons { get; set; }

        #endregion


    }
}