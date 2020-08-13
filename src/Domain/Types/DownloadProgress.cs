using Newtonsoft.Json;

namespace PlexRipper.Domain.Types
{
    public class DownloadProgress
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public long DownloadSpeed { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        /// <summary>
        /// The download time remaining in seconds
        /// </summary>
        [JsonProperty("timeRemaining", Required = Required.Always)]
        public int TimeRemaining { get; set; }

    }
}
