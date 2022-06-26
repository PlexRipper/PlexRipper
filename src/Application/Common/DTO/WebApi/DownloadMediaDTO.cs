using Newtonsoft.Json;

namespace PlexRipper.Application
{
    public class DownloadMediaDTO
    {
        [JsonProperty("mediaIds", Required = Required.Always)]
        public List<int> MediaIds { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("plexAccountId", Required = Required.Always)]
        public int PlexAccountId { get; set; }
    }
}