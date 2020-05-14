using Newtonsoft.Json;
using System;

namespace PlexRipper.Infrastructure.Common.DTO.PlexGetServer
{
    public class PlexServerDTO
    {

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("scheme")]
        public string Scheme { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("localAddresses")]
        public string LocalAddresses { get; set; }

        [JsonProperty("machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("owned")]
        public bool Owned { get; set; }

        [JsonProperty("synced")]
        public bool Synced { get; set; }

        [JsonProperty("ownerId")]
        public int OwnerId { get; set; }

        [JsonProperty("home")]
        public bool Home { get; set; }
    }

}
