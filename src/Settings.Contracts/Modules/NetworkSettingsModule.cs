using Flurl;
using Reaparr.Environment;

namespace Reaparr.Settings.Contracts;

public record NetworkSettingsModule
    : BaseSettingsModule<NetworkSettingsModule>,
        IBaseSettingsModule<NetworkSettingsModule>,
        INetworkSettings
{
    public static NetworkSettingsModule Create() =>
        new()
        {
            ReverseProxyUrl = string.Empty,
            BasePath = string.Empty,
            TrustProxyHeaders = false,
            AllowedProxyIps = [],
            ForwardedHostHeader = string.Empty,
            ForwardedPathHeader = string.Empty,
        };

    public required string ReverseProxyUrl
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public required string BasePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public required bool TrustProxyHeaders
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required List<string> AllowedProxyIps
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    public required string ForwardedHostHeader
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public required string ForwardedPathHeader
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public Uri Uri => new(Url);

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
