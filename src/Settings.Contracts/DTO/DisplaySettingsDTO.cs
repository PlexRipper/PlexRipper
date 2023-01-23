using Newtonsoft.Json;
using PlexRipper.Domain;

namespace Settings.Contracts;

public class DisplaySettingsDTO : IDisplaySettings
{
    [JsonProperty(Required = Required.Always)]
    public ViewMode TvShowViewMode { get; set; }

    [JsonProperty(Required = Required.Always)]
    public ViewMode MovieViewMode { get; set; }
}