namespace Reaparr.Settings.Contracts;

/// <summary>
/// DTO for header-based authentication configuration
/// </summary>
public record HeaderAuthenticationSettingsDTO : IHeaderAuthenticationSettings
{
    /// <summary>
    /// Whether header-based authentication is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// How to map the header value to a Reaparr user (by username, email, or custom mapping)
    /// </summary>
    public HeaderMappingType MappingType { get; set; } = HeaderMappingType.Username;

    /// <summary>
    /// Custom mapping function (when MappingType is Custom)
    /// </summary>
    public string? CustomMappingExpression { get; set; }

    /// <summary>
    /// Whether to auto-create users if they don't exist
    /// </summary>
    public bool AutoCreateUsers { get; set; }

    /// <summary>
    /// Default role to assign to auto-created users
    /// </summary>
    public string DefaultRole { get; set; } = "Admin";

    /// <summary>
    /// List of trusted proxy IP addresses or CIDR ranges
    /// </summary>
    public List<string> TrustedProxies { get; set; } = new();

    /// <summary>
    /// Whether to log header-based authentication attempts
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Maximum length for header values to prevent abuse
    /// </summary>
    public int MaxHeaderLength { get; set; } = 256;

    /// <summary>
    /// Whether to require HTTPS for header-based authentication
    /// </summary>
    public bool RequireHttps { get; set; } = true;
}
