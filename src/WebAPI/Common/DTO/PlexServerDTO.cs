using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexServerDTO
    {

        [JsonProperty("id", Required = Required.DisallowNull)]
        public int Id { get; set; }

        [JsonProperty("accessToken", Required = Required.DisallowNull)]
        public string AccessToken { get; set; }

        [JsonProperty("name", Required = Required.DisallowNull)]
        public string Name { get; set; }

        [JsonProperty("address", Required = Required.DisallowNull)]
        public string Address { get; set; }

        [JsonProperty("port", Required = Required.DisallowNull)]
        public int Port { get; set; }

        [JsonProperty("version", Required = Required.DisallowNull)]
        public string Version { get; set; }

        [JsonProperty("scheme", Required = Required.DisallowNull)]
        public string Scheme { get; set; }

        [JsonProperty("host", Required = Required.DisallowNull)]
        public string Host { get; set; }

        [JsonProperty("localAddresses", Required = Required.DisallowNull)]
        public string LocalAddresses { get; set; }

        [JsonProperty("machineIdentifier", Required = Required.DisallowNull)]
        public string MachineIdentifier { get; set; }

        [JsonProperty("createdAt", Required = Required.DisallowNull)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt", Required = Required.DisallowNull)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("owned", Required = Required.DisallowNull)]
        public bool Owned { get; set; }

        [JsonProperty("synced", Required = Required.DisallowNull)]
        public bool Synced { get; set; }

        [JsonProperty("ownerId", Required = Required.DisallowNull)]
        public int OwnerId { get; set; }

        [JsonProperty("home", Required = Required.DisallowNull)]
        public bool Home { get; set; }

        [JsonProperty("plexLibraries", Required = Required.DisallowNull)]
        public List<PlexLibraryDTO> PlexLibraries { get; set; }

    }

}
