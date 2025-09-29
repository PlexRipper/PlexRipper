using System.Net;

namespace Reaparr.Settings.Contracts;

/// <summary>
/// Configuration for header-based authentication
/// </summary>
public record HeaderAuthenticationSettings
    : BaseSettingsModule<HeaderAuthenticationSettings>,
        IHeaderAuthenticationSettings
{
    private bool _enabled;
    private HeaderMappingType _mappingType = HeaderMappingType.Username;
    private List<string> _trustedProxies = new();
    private bool _enableLogging = true;
    private int _maxHeaderLength = 256;
    private bool _requireHttps = true;

    /// <summary>
    /// Whether header-based authentication is enabled
    /// </summary>
    public required bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    /// <summary>
    /// How to map the header value to a Reaparr user (by username, email, or custom mapping)
    /// </summary>
    public required HeaderMappingType MappingType
    {
        get => _mappingType;
        set => SetProperty(ref _mappingType, value);
    }

    /// <summary>
    /// List of trusted proxy IP addresses or CIDR ranges
    /// </summary>
    public required List<string> TrustedProxies
    {
        get => _trustedProxies;
        set => SetProperty(ref _trustedProxies, value);
    }

    /// <summary>
    /// Whether to log header-based authentication attempts
    /// </summary>
    public required bool EnableLogging
    {
        get => _enableLogging;
        set => SetProperty(ref _enableLogging, value);
    }

    /// <summary>
    /// Maximum length for header values to prevent abuse
    /// </summary>
    public required int MaxHeaderLength
    {
        get => _maxHeaderLength;
        set => SetProperty(ref _maxHeaderLength, value);
    }

    /// <summary>
    /// Whether to require HTTPS for header-based authentication
    /// </summary>
    public required bool RequireHttps
    {
        get => _requireHttps;
        set => SetProperty(ref _requireHttps, value);
    }

    /// <summary>
    /// Creates a new instance with default values
    /// </summary>
    public static HeaderAuthenticationSettings Create() =>
        new()
        {
            Enabled = false,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = new List<string>(),
            EnableLogging = true,
            MaxHeaderLength = 256,
            RequireHttps = true,
        };

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    public bool IsValid()
    {
        if (!Enabled)
            return true;

        if (MaxHeaderLength <= 0)
            return false;

        // Validate trusted proxies are valid IP addresses or CIDR ranges
        foreach (var proxy in TrustedProxies)
        {
            if (string.IsNullOrWhiteSpace(proxy))
                return false;

            // Check if it's a valid IP or CIDR
            if (!IsValidIpOrCidr(proxy))
                return false;
        }

        return true;
    }

    private static bool IsValidIpOrCidr(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Check for CIDR notation (e.g., 192.168.1.0/24)
        if (input.Contains('/'))
        {
            var parts = input.Split('/');
            if (parts.Length != 2)
                return false;

            return IPAddress.TryParse(parts[0], out _)
                && int.TryParse(parts[1], out var cidr)
                && cidr >= 0
                && cidr <= 32;
        }

        // Check for single IP address
        return IPAddress.TryParse(input, out _);
    }
}
