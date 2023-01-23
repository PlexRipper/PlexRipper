using Newtonsoft.Json;

namespace Settings.Contracts;

public class LanguageSettingsDTO : ILanguageSettings
{
    [JsonProperty(Required = Required.Always)]
    public string Language { get; set; }
}