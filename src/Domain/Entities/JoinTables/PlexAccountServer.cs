using PlexRipper.Domain.Entities.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities.JoinTables
{
    public class PlexAccountServer : BaseEntity
    {
        #region Relationships
        [Column(Order = 1)]
        public int PlexAccountId { get; set; }
        public virtual PlexAccount PlexAccount { get; set; }

        [Column(Order = 2)]
        public int PlexServerId { get; set; }
        public virtual PlexServer PlexServer { get; set; }
        #endregion

        [Column(Order = 3)]
        public string AuthToken { get; set; }
        [Column(Order = 4)]
        public DateTime AuthTokenCreationDate { get; set; }

    }
}
