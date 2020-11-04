using System;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.SignalR.Common
{
    public class NotificationUpdate
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("level", Required = Required.Always)]
        public NotificationLevel Level { get; set; }

        [JsonProperty("createdAt", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; }

        [JsonProperty("hidden", Required = Required.Always)]
        public bool Hidden { get; set; }
    }
}