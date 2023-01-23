using Newtonsoft.Json;
using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.WebAPI.Common.DTO;

public class LanguageSettingsDTO : ILanguageSettings
{
    [JsonProperty(Required = Required.Always)]
    public string Language { get; set; }
}