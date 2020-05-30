using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexSerie : PlexMedia
    {
        public virtual List<PlexSerieGenre> PlexSerieGenres { get; set; }
        public virtual List<PlexSerieRole> PlexSerieRoles { get; set; }
    }
}
