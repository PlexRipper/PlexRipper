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
        public string AccessToken { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Version { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string LocalAddresses { get; set; }
        public string MachineIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Owned { get; set; }
        public bool Synced { get; set; }
        public long OwnerId { get; set; }
        public bool Home { get; set; }

        [IgnoreMap]
        public virtual List<PlexAccountServer> PlexAccountServers { get; set; }

        public virtual List<PlexLibrary> PlexLibraries { get; set; }

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

        #endregion

    }
}
