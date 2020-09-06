using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexTvShow : PlexMedia
    {
        public PlexMediaType Type => PlexMediaType.TvShow;
        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }
        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }
        public List<PlexTvShowSeason> Seasons { get; set; }
    }
}
