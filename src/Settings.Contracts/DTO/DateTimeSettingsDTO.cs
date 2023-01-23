using Newtonsoft.Json;

namespace Settings.Contracts;

public class DateTimeSettingsDTO : IDateTimeSettings
{
    [JsonProperty(Required = Required.Always)]
    public string ShortDateFormat { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string LongDateFormat { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string TimeFormat { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string TimeZone { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool ShowRelativeDates { get; set; }
}