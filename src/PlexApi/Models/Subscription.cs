using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models;

public class Subscription
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("subscribedAt")]
    public DateTime SubscribedAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("paymentService")]
    public object PaymentService { get; set; }

    [JsonPropertyName("plan")]
    public object Plan { get; set; }

    [JsonPropertyName("features")]
    public List<string> Features { get; set; }
}