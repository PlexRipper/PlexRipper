using Newtonsoft.Json;

namespace PlexRipper.Application.Common.WebApi
{
    public class InspectServerProgress
    {
        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("attemptIndex", Required = Required.Always)]
        public int AttemptIndex { get; set; }

        [JsonProperty("attemptCount", Required = Required.Always)]
        public int AttemptCount { get; set; }

        [JsonProperty("connectionSuccessful", Required = Required.Always)]
        public bool ConnectionSuccessful { get; set; }

        [JsonProperty("serverFixApplyDNSFix", Required = Required.Always)]
        public bool ServerFixApplyDNSFix { get; set; }
    }
}