namespace PlexRipper.PlexApi;

public static class PlexApiPaths
{
    #region Methods

    #region Private

    private static string ServerUrl(string url)
    {
        return url.TrimEnd('/');
    }

    #endregion

    #region Public

    #region Urls

    public static string ServerIdentity(string serverUrl)
    {
        return $"{ServerUrl(serverUrl)}{ServerIdentityPath}";
    }

    public static string GetLibraries(string serverUrl)
    {
        return $"{ServerUrl(serverUrl)}{LibrarySectionsPath}";
    }

    public static string GetLibrariesMetadata(string serverUrl, string libraryKey)
    {
        return $"{ServerUrl(serverUrl)}{GetLibrariesSectionsPath(libraryKey)}";
    }

    #endregion

    #region Relative

    public static string GetLibrariesSectionsPath(string libraryKey)
    {
        return $"{LibrarySectionsPath}/{libraryKey}/all";
    }

    #endregion

    #endregion

    #endregion

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
}
