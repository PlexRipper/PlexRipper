using Newtonsoft.Json;
using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.WebAPI.Common.DTO;

public class GeneralSettingsDTO : IGeneralSettings
{
    [JsonProperty(Required = Required.Always)]
    public bool FirstTimeSetup { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int ActiveAccountId { get; set; }
}