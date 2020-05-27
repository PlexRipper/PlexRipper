using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexRole : BaseEntity
    {
        public string Tag { get; set; }
        public virtual List<PlexMovieRole> PlexMovieRoles { get; set; }
    }
}
