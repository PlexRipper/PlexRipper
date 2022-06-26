using Newtonsoft.Json;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DisplaySettingsDTO : IDisplaySettings
    {
        [JsonProperty(Required = Required.Always)]
        public ViewMode TvShowViewMode { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ViewMode MovieViewMode { get; set; }
    }
}