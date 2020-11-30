using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
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

        public long OwnerId { get; set; }

        #region Relationships

        public List<PlexAccountServer> PlexAccountServers { get; set; } = new List<PlexAccountServer>();

        public List<PlexLibrary> PlexLibraries { get; set; } = new List<PlexLibrary>();

        public List<PlexServerStatus> ServerStatus { get; set; } = new List<PlexServerStatus>();

        #endregion

        #region Helpers

        /// <summary>
        /// The server url, e.g: http://112.202.10.213:32400
        /// </summary>
        [NotMapped]
        public string ServerUrl => $"{Scheme}://{Address}:{Port}";

        /// <summary>
        /// The library section url derived from the BaseUrl, e.g: http://112.202.10.213:32400/library/sections
        /// </summary>
        [NotMapped]
        public string LibraryUrl => $"{ServerUrl}/library/sections";

        /// <summary>
        /// Do not use this property to retrieve the needed authToken, this is only meant to transfer the incoming authToken from the plexApi to the Database.
        /// See AddOrUpdatePlexServersHandler.
        /// </summary>
        [NotMapped]
        public string AccessToken { get; set; }

        /// <summary>
        /// Check if any nested <see cref="PlexLibrary"/> has a <see cref="DownloadTask"/>.
        /// </summary>
        [NotMapped]
        public bool HasDownloadTasks => PlexLibraries?.Any(x => x.DownloadTasks?.Any() ?? false) ?? false;

        #endregion
    }
}