using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShow : PlexMedia
    {
        public virtual List<PlexTvShowGenre> PlexTvShowGenres { get; set; }
        public virtual List<PlexTvShowRole> PlexTvShowRoles { get; set; }
    }
}
