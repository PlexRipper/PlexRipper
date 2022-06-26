using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO;

public class SubscriptionDTO
{
    [JsonProperty("active")]
    public bool Active { get; set; }

    [JsonProperty("status")]
    public object Status { get; set; }

    [JsonProperty("plan")]
    public object Plan { get; set; }

    [JsonProperty("features")]
    public object Features { get; set; }
}