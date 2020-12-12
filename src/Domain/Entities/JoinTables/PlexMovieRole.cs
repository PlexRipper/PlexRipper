namespace PlexRipper.Domain
{
    public class PlexMovieRole : BaseEntity
    {
        public int PlexGenreId { get; set; }

        public virtual PlexGenre PlexGenre { get; set; }

        public int PlexMoviesId { get; set; }

        public virtual PlexMovie PlexMovie { get; set; }
    }
}