using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities
{
    public class PlexAccount : BaseEntity
    {
        [Column(Order = 1)]
        public string DisplayName { get; set; }
        [Column(Order = 2)]
        public string Username { get; set; }
        [Column(Order = 3)]
        public string Password { get; set; }
        [Column(Order = 4)]
        public bool IsEnabled { get; set; }
        [Column(Order = 5)]
        public bool IsValidated { get; set; }
        [Column(Order = 6)]
        public DateTime ValidatedAt { get; set; }

        public long PlexId { get; set; }
        public string Uuid { get; set; }
        public string Email { get; set; }
        public DateTime JoinedAt { get; set; }
        public string Title { get; set; }
        // public Uri Thumb { get; set; }
        public bool HasPassword { get; set; }
        /// <summary>
        /// The general plex authentication token used to retrieve account data such as the <see cref="PlexServer"/>s the account has access to. 
        /// </summary>
        public string AuthenticationToken { get; set; }
        public int ForumId { get; set; }

        #region Relationships

        /// <summary>
        /// The associated PlexAccountServers the user has access to
        /// </summary>
        public virtual ICollection<PlexAccountServer> PlexAccountServers { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public List<PlexServer> PlexServers { get; set; }

        /// <summary>
        /// This merges the response of the PlexApi into this <see cref="PlexAccount"/>
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> from the PlexApi</param>
        public void FromPlexApi(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning("The plexAccount was null");
                return;
            }
            if (plexAccount.AuthenticationToken == string.Empty)
            {
                Log.Warning("The plexAccount has an invalid AuthenticationToken and was most likely not valid");
                return;
            }

            PlexId = plexAccount.PlexId;
            Uuid = plexAccount.Uuid;
            JoinedAt = plexAccount.JoinedAt;
            Title = plexAccount.Title;
            HasPassword = plexAccount.HasPassword;
            AuthenticationToken = plexAccount.AuthenticationToken;
            ForumId = plexAccount.ForumId;
            IsValidated = true;
            ValidatedAt = DateTime.Now;
        }

        public PlexAccount()
        {

        }

        public PlexAccount(string username, string password)
        {
            Username = username;
            Password = password;
            IsEnabled = true;
        }
        #endregion
    }
}
