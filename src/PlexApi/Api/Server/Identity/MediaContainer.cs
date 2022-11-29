using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Api;

public class ServerIdentityResponse
{
    [JsonPropertyName("MediaContainer")]
    public ServerIdentityResponseMediaContainer MediaContainer { get; set; }
}

public class ServerIdentityResponseMediaContainer
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("claimed")]
    public bool Claimed { get; set; }

    [JsonPropertyName("machineIdentifier")]
    public string MachineIdentifier { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}