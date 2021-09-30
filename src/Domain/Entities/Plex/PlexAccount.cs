using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FluentResultExtensions.lib;
using Logging;

namespace PlexRipper.Domain
{
    /// <summary>
    /// The Plex Account entity in the database.
    /// </summary>
    public class PlexAccount : BaseEntity
    {
        private string _clientId;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexAccount"/> class.
        /// </summary>
        public PlexAccount() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexAccount"/> class.
        /// </summary>
        /// <param name="username">The username to use.</param>
        /// <param name="password">The password to use.</param>
        public PlexAccount(string username, string password)
        {
            Username = username;
            Password = password;
            IsEnabled = true;
        }

        #endregion

        #region Properties

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

        [Column(Order = 7)]
        public long PlexId { get; set; }

        [Column(Order = 8)]
        public string Uuid { get; set; }

        /// <summary>
        /// The unique client identifier used for all PlexApi communication.
        /// </summary>
        [Column(Order = 9)]
        public string ClientId
        {
            get => string.IsNullOrEmpty(this._clientId) ? GenerateClientId() : _clientId;
            private set => _clientId = value;
        }

        public string Email { get; set; }

        public DateTime JoinedAt { get; set; }

        public string Title { get; set; }

        // public Uri Thumb { get; set; }
        public bool HasPassword { get; set; }

        /// <summary>
        ///     The general plex authentication token used to retrieve account data such as the <see cref="PlexServer" />s the
        ///     account has access to.
        /// </summary>
        public string AuthenticationToken { get; set; }

        /// <summary>
        /// If this is a main account then it will get a lower priority when downloading media which a non-main account also has access to.
        /// </summary>
        public bool IsMain { get; set; }

        #region Relationships

        /// <summary>
        ///     The associated PlexAccountServers the user has access to.
        /// </summary>
        public List<PlexAccountServer> PlexAccountServers { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public List<PlexServer> PlexServers => PlexAccountServers.Select(x => x.PlexServer).ToList();

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Generates an unique 24 character ClientId and sets it to this <see cref="PlexAccount"/>
        /// </summary>
        /// <returns>The generated ClientId.</returns>
        public string GenerateClientId()
        {
            this._clientId = StringExtensions.RandomString(24, true, true);
            return this._clientId;
        }

        /// <summary>
        ///     This merges the response of the PlexApi into this <see cref="PlexAccount" />.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount" /> from the PlexApi.</param>
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
            IsValidated = true;
            ValidatedAt = DateTime.Now;
        }

        #endregion
    }
}