using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class ThumbnailRequestDTO
    {
        [JsonProperty("plexMediaId", Required = Required.Always)]
        public int PlexMediaId { get; set; }

        [JsonProperty("width", Required = Required.Always)]
        public int Width { get; set; }

        [JsonProperty("height", Required = Required.Always)]
        public int Height { get; set; }

        [JsonProperty("plexMediaType", Required = Required.Always)]
        public PlexMediaType PlexMediaType { get; set; }
    }
}