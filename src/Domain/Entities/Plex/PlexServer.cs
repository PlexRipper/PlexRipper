using AutoMapper;
using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Entities.JoinTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities
{
    public class PlexServer : BaseEntity
    {
        [Column(Order = 1)]
        public string Name { get; set; }
        [Column(Order = 2)]
        public string Scheme { get; set; }
        [Column(Order = 3)]
        public string Address { get; set; }
        [Column(Order = 4)]
        public int Port { get; set; }
        [Column(Order = 5)]
        public string Version { get; set; }
        [Column(Order = 6)]
        public string Host { get; set; }
        [Column(Order = 7)]
        public string LocalAddresses { get; set; }
        [Column(Order = 8)]
        public string MachineIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Owned { get; set; }
        public bool Synced { get; set; }
        public long OwnerId { get; set; }
        public bool Home { get; set; }


        #region Relationships

        [IgnoreMap] // TODO remove IgnoreMap
        public virtual ICollection<PlexAccountServer> PlexAccountServers { get; set; }

        public virtual ICollection<PlexLibrary> PlexLibraries { get; set; }

        public virtual ICollection<PlexServerStatus> ServerStatus { get; set; }
        #endregion

        #region Helpers


        /// <summary>
        /// The server url, e.g: http://112.202.10.213:32400
        /// </summary>
        [NotMapped]
        public string BaseUrl => $"{Scheme}://{Address}:{Port}";

        /// <summary>
        /// The library section url derived from the BaseUrl, e.g: http://112.202.10.213:32400/library/sections
        /// </summary>
        [NotMapped]
        public string LibraryUrl => $"{BaseUrl}/library/sections";

        [NotMapped]
        public string AccessToken { get; set; }
        #endregion

    }
}
