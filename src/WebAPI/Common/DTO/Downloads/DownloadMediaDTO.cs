using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadMediaDTO
    {
        [JsonProperty("mediaIds", Required = Required.Always)]
        public List<int> MediaIds { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("libraryId", Required = Required.Always)]
        public int LibraryId { get; set; }

        [JsonProperty("plexAccountId", Required = Required.Always)]
        public int PlexAccountId { get; set; }
    }
}