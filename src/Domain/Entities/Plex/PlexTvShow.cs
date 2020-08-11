using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Domain.Entities
{
    public class PlexTvShow : PlexMedia
    {
        public PlexMediaType Type => PlexMediaType.TvShow;
        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }
        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }
        public List<PlexTvShowSeason> Seasons { get; set; }
    }
}
