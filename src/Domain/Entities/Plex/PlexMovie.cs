using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexMovie : PlexMedia
    {
        public List<PlexMovieGenre> PlexMovieGenres { get; set; }
        public List<PlexMovieRole> PlexMovieRoles { get; set; }

    }
}
