using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadTvShowDTO
    {
        [JsonProperty("plexAccountId", Required = Required.Always)]
        public int PlexAccountId { get; set; }

        [JsonProperty("plexMediaId", Required = Required.Always)]
        public int PlexMediaId { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }
    }
}