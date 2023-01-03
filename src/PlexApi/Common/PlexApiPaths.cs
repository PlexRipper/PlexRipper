namespace PlexRipper.PlexApi.Common;

public static class PlexApiPaths
{
    #region Properties

    #region Base

    public static string PlexUrl => $"https://{Host}";

    public static string Host => "plex.tv";

    #endregion

    #region Relative

    public static string SignInPath => "/api/v2/users/signin";

    public static string ServerResourcesPath => "/api/v2/resources";

    public static string AuthPinPath => "/api/v2/pins";

    public static string ServerIdentityPath => "/identity";

    public static string LibrarySectionsPath => "/library/sections";

    #endregion

    #region Absolute

    public static string SignInUrl => $"{PlexUrl}{SignInPath}";

    public static string ServerResourcesUrl => $"{PlexUrl}{ServerResourcesPath}";

    public static string PlexPinUrl => $"{PlexUrl}{AuthPinPath}";

    #endregion

    #endregion

    public static string ServerIdentity(string serverUrl)
    {
        return $"{serverUrl.TrimEnd('/')}{ServerIdentityPath}";
    }

    public static string GetLibraries(string serverUrl)
    {
        return $"{serverUrl.TrimEnd('/')}{LibrarySectionsPath}";
    }
}