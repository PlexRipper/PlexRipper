using PlexRipper.Domain.Entities.Plex;
using System;

namespace PlexRipper.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public virtual PlexAccount PlexAccount { get; set; }
        public long? PlexAccountId { get; set; }
    }
}
