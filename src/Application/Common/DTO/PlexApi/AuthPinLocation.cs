using Newtonsoft.Json;

namespace PlexRipper.Application;

public class AuthPinLocation
{
    [JsonProperty(Required = Required.Always)]
    public string Code { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool EuropeanUnionMember { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string ContinentCode { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Country { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string City { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string TimeZone { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string PostalCode { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Subdivisions { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Coordinates { get; set; }
}