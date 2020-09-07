using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class ThumbnailRequestDTO
    {
        [JsonProperty("plexAccountId", Required = Required.Always)]
        public int PlexAccountId { get; set; }

        [JsonProperty("plexMediaId", Required = Required.Always)]
        public int PlexMediaId { get; set; }

        [JsonProperty("plexMediaType", Required = Required.Always)]
        public PlexMediaType PlexMediaType { get; set; }
    }
}