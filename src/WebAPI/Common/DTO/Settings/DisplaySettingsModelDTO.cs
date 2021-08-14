using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DisplaySettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public ViewMode TvShowViewMode { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ViewMode MovieViewMode { get; set; }
    }
}