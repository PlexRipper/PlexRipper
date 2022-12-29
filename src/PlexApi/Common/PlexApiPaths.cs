namespace PlexRipper.PlexApi.Common;

public static class PlexApiPaths
{
    private static readonly UriBuilder PlexUrl;

    static PlexApiPaths()
    {
        PlexUrl = new UriBuilder()
        {
            Scheme = "https",
            Host = "plex.tv",
        };
    }

    public static string Host => PlexUrl.Host;

    public static string SignInPath => "api/v2/users/signin";

    public static string SignInUrl => $"{PlexUrl}{SignInPath}";

    public static string PlexServerUrl => $"{PlexUrl}api/v2/resources";

    public static string PlexPinUrl => $"{PlexUrl}api/v2/pins";
}