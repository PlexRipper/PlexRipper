namespace PlexRipper.PlexApi.Common;

public static class PlexApiPaths
{
    #region Constructors

    static PlexApiPaths()
    {
        PlexUrl = new UriBuilder()
        {
            Scheme = "https",
            Host = "plex.tv",
        };
    }

    #endregion

    #region Properties

    public static string Host => PlexUrl.Host;

    #region Relative

    public static string SignInPath => "api/v2/users/signin";

    public static string ServerResourcesPath => "api/v2/resources";

    public static string ServerIdentityPath => "/identity";

    #endregion

    #region Absolute

    public static string SignInUrl => $"{PlexUrl}{SignInPath}";

    public static string ServerResourcesUrl => $"{PlexUrl}{ServerResourcesPath}";

    public static string PlexPinUrl => $"{PlexUrl}api/v2/pins";

    #endregion

    #endregion

    private static readonly UriBuilder PlexUrl;

    public static string ServerIdentity(string serverUrl)
    {
        return $"{serverUrl.TrimEnd('/')}{ServerIdentityPath}";
    }
}