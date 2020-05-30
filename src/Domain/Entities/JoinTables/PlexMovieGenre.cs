using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities.JoinTables
{
    public class PlexMovieGenre : BaseEntity
    {
        public int PlexGenreId { get; set; }
        public virtual PlexGenre PlexGenre { get; set; }

        public int PlexMoviesId { get; set; }
        public virtual PlexMovie PlexMovie { get; set; }

    }
}
