using Newtonsoft.Json;

namespace PlexRipper.WebAPI.SignalR.Common
{
    public class DownloadTaskCreationProgress
    {
        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("current", Required = Required.Always)]
        public int Current { get; set; }

        [JsonProperty("total", Required = Required.Always)]
        public int Total { get; set; }

        /// <summary>
        /// Has the library finished refreshing.
        /// </summary>
        [JsonProperty("isComplete", Required = Required.Always)]
        public bool IsComplete { get; set; }
    }
}