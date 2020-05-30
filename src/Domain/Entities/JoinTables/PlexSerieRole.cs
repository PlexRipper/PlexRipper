using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities.JoinTables
{
    public class PlexSerieRole : BaseEntity
    {
        public int PlexGenreId { get; set; }
        public virtual PlexGenre PlexGenre { get; set; }

        public int PlexSerieId { get; set; }
        public virtual PlexSerie PlexSerie { get; set; }

    }
}
