using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
        /// </summary>
        [JsonProperty("mediaType", Required = Required.Always)]
        public PlexMediaType MediaType { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public long DownloadSpeed { get; set; }

        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining { get; set; }

        [JsonProperty("actions", Required = Required.Always)]
        public List<string> Actions { get; set; }

        [JsonProperty("children", Required = Required.Always)]
        public List<DownloadProgressDTO> Children { get; set; }
    }
}