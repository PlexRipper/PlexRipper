using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities
{
    public class PlexServer
    {
        public int Id { get; set; }
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
        public string SourceTitle { get; set; }
        public long OwnerId { get; set; }
        public bool Home { get; set; }

        public virtual List<PlexAccountServer> PlexAccountServers { get; set; }
        public virtual List<PlexLibrary> PlexLibraries { get; set; }


        [NotMapped]
        public string BaseUrl => $"{Scheme}://{Address}:{Port}";

        [NotMapped]
        public string LibraryUrl => $"{BaseUrl}/library/sections";
    }
}
