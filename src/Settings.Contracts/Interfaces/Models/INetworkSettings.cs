namespace Reaparr.Settings.Contracts;

/// <summary>
/// Settings used to configure reverse proxy behavior and forwarded headers.
/// </summary>
public interface INetworkSettings
{
    /// <summary>
    /// Public base URL used when generating external links (empty uses localhost).
    /// </summary>
    string ReverseProxyUrl { get; set; }

    /// <summary>
    /// Optional path prefix used by the reverse proxy (e.g. /reaparr).
    /// </summary>
    string BasePath { get; set; }

    /// <summary>
    /// Enables trusting forwarded headers from an upstream reverse proxy.
    /// </summary>
    bool TrustProxyHeaders { get; set; }

    /// <summary>
    /// Allowed proxy IPs or CIDR ranges that may send forwarded headers.
    /// </summary>
    List<string> AllowedProxyIps { get; set; }

    /// <summary>
    /// Custom header name for the forwarded host (defaults to X-Forwarded-Host).
    /// </summary>
    string ForwardedHostHeader { get; set; }

    /// <summary>
    /// Custom header name for the forwarded path prefix (defaults to X-Forwarded-Prefix).
    /// </summary>
    string ForwardedPathHeader { get; set; }

    /// <summary>
    /// Returns the base URL using reverse proxy settings or localhost when unset.
    /// </summary>
    string Url { get; }
}
