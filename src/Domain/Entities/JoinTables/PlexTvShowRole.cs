using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities.JoinTables
{
    public class PlexTvShowRole : BaseEntity
    {
        public int PlexGenreId { get; set; }
        public virtual PlexGenre PlexGenre { get; set; }

        public int PlexTvShowId { get; set; }
        public virtual PlexTvShow PlexTvShow { get; set; }

    }
}
