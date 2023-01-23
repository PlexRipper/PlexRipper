using Newtonsoft.Json;

namespace Settings.Contracts;

public class GeneralSettingsDTO : IGeneralSettings
{
    [JsonProperty(Required = Required.Always)]
    public bool FirstTimeSetup { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int ActiveAccountId { get; set; }
}