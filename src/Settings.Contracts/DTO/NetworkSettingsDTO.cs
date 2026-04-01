using Flurl;

namespace Reaparr.Settings.Contracts;

public class NetworkSettingsDTO : INetworkSettings
{
    public required string ReverseProxyUrl { get; set; }

    public required string BasePath { get; set; }

    public required bool TrustProxyHeaders { get; set; }

    public required List<string> AllowedProxyIps { get; set; }

    public required string ForwardedHostHeader { get; set; }

    public required string ForwardedPathHeader { get; set; }

    [JsonIgnore]
    public Uri Uri => new(Url);

    [JsonIgnore]
    public string Url
    {
        get
        {
            if (
                !string.IsNullOrWhiteSpace(ReverseProxyUrl)
                && Uri.TryCreate(ReverseProxyUrl, UriKind.Absolute, out var reverseProxyUri)
            )
            {
                return reverseProxyUri.AppendPathSegment(BasePath).ToString();
            }

            return new UriBuilder { Host = "localhost", Port = EnvironmentExtensions.GetPort }.ToString();
        }
    }
}
