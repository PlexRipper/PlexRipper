using System;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexAccount
    {
        public long Id { get; set; }
        public string Uuid { get; set; }
        public string Email { get; set; }
        public DateTime JoinedAt { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        // public Uri Thumb { get; set; }
        public bool HasPassword { get; set; }
        public string AuthToken { get; set; }
        public string AuthenticationToken { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public int ForumId { get; set; }

        /// <summary>
        /// The Plex Ripper account associated with this Plex account.
        /// </summary>
        public virtual Account Account { get; set; }
        public int AccountId { get; set; }

        /// <summary>
        /// The associated PlexAccountServers the user has access to
        /// </summary>
        public virtual List<PlexAccountServer> PlexAccountServers { get; set; }
    }
}
