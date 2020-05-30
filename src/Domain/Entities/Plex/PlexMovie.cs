using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexMovie : PlexMedia
    {
        public virtual List<PlexMovieGenre> PlexMovieGenres { get; set; }
        public virtual List<PlexMovieRole> PlexMovieRoles { get; set; }

    }
}
