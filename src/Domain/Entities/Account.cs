using PlexRipper.Domain.Entities.Base;
using System;

namespace PlexRipper.Domain.Entities
{
    /// <summary>
    /// This is used as an account wrapper around <see cref="PlexAccount"/>
    /// </summary>
    public class Account : BaseEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsValidated { get; set; }
        public DateTime ValidatedAt { get; set; }
        public virtual PlexAccount PlexAccount { get; set; }

        public Account()
        {

        }

        public Account(string username, string password)
        {
            Username = username;
            Password = password;
            IsEnabled = true;
        }
    }
}
