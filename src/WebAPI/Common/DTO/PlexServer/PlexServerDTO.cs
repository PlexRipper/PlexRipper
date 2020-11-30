using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexServerDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("address", Required = Required.Always)]
        public string Address { get; set; }

        [JsonProperty("port", Required = Required.Always)]
        public int Port { get; set; }

        [JsonProperty("version", Required = Required.Always)]
        public string Version { get; set; }

        [JsonProperty("scheme", Required = Required.Always)]
        public string Scheme { get; set; }

        [JsonProperty("host", Required = Required.Always)]
        public string Host { get; set; }

        [JsonProperty("localAddresses", Required = Required.Always)]
        public string LocalAddresses { get; set; }

        [JsonProperty("serverUrl", Required = Required.Always)]
        public string ServerUrl { get; set; }

        [JsonProperty("machineIdentifier", Required = Required.Always)]
        public string MachineIdentifier { get; set; }

        [JsonProperty("createdAt", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("ownerId", Required = Required.Always)]
        public int OwnerId { get; set; }

        [JsonProperty("plexLibraries", Required = Required.Always)]
        public List<PlexLibraryDTO> PlexLibraries { get; set; }
    }
}