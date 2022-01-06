using Newtonsoft.Json;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class GeneralSettingsDTO : IGeneralSettings
    {
        [JsonProperty(Required = Required.Always)]
        public bool FirstTimeSetup { get; set; }
    }
}