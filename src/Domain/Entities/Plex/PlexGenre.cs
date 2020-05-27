using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexGenre : BaseEntity
    {
        public string Tag { get; set; }

        public virtual List<PlexMovieGenre> PlexMovies { get; set; }

    }
}
