namespace Reaparr.Settings.Contracts;

/// <summary>
/// Configuration for header-based authentication
/// </summary>
public interface IHeaderAuthenticationSettings
{
    /// <summary>
    /// Whether header-based authentication is enabled
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// The name of the HTTP header to trust for user identity
    /// </summary>
    string HeaderName { get; set; }

    /// <summary>
    /// How to map the header value to a Reaparr user (by username, email, or custom mapping)
    /// </summary>
    HeaderMappingType MappingType { get; set; }

    /// <summary>
    /// Custom mapping function (when MappingType is Custom)
    /// </summary>
    string? CustomMappingExpression { get; set; }

    /// <summary>
    /// Whether to auto-create users if they don't exist
    /// </summary>
    bool AutoCreateUsers { get; set; }

    /// <summary>
    /// Default role to assign to auto-created users
    /// </summary>
    string DefaultRole { get; set; }

    /// <summary>
    /// List of trusted proxy IP addresses or CIDR ranges
    /// </summary>
    List<string> TrustedProxies { get; set; }

    /// <summary>
    /// Whether to log header-based authentication attempts
    /// </summary>
    bool EnableLogging { get; set; }

    /// <summary>
    /// Maximum length for header values to prevent abuse
    /// </summary>
    int MaxHeaderLength { get; set; }

    /// <summary>
    /// Whether to require HTTPS for header-based authentication
    /// </summary>
    bool RequireHttps { get; set; }
}

/// <summary>
/// How to map header values to Reaparr users
/// </summary>
public enum HeaderMappingType
{
    /// <summary>
    /// Map header value directly to username
    /// </summary>
    Username = 0,

    /// <summary>
    /// Map header value to email address
    /// </summary>
    Email = 1,

    /// <summary>
    /// Use custom mapping expression
    /// </summary>
    Custom = 2,
}
