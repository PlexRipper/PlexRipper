using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShow : PlexMedia
    {
        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }
        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }
        public List<PlexTvShowSeason> Seasons { get; set; }
    }
}
