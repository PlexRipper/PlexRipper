namespace Reaparr.AppHost;

/// <summary>
/// Defines constant header keys for Reaparr API requests. These headers are used to identify the client application version and platform when making requests to the Reaparr API.
/// </summary>
public static class ReaparrHeaders
{
    /// <summary>
    /// The header key for the Reaparr application version. This should be included in API requests to identify the client version making the request.
    /// </summary>
    public const string Version = "X-Reaparr-Version";

    /// <summary>
    /// The header key for the Reaparr application platform. This should be included in API requests to identify the client platform (e.g., desktop, docker) making the request.
    /// </summary>
    public const string Platform = "X-Reaparr-Platform";
}
