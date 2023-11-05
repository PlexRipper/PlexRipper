#nullable enable
namespace Environment;

public static class EnvironmentExtensions
{
    #region Fields

    public const string IntegrationTestModeKey = "IntegrationTestMode";

    #endregion

    #region Public Methods

    #region Get

    public static bool IsIntegrationTestMode()
    {
        return System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == Convert.ToString(true);
    }

    /// <summary>
    /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot PlexRipper in development mode in a non-docker environment.
    /// </summary>
    public static string? GetDevelopmentRootPath()
    {
        return System.Environment.GetEnvironmentVariable("DEVELOPMENT_ROOT_PATH");
    }

    /// <summary>
    /// Get root path from env variable so we can adjust it per environment.
    /// </summary>
    public static string? GetPlexRipperRootPath()
    {
        return System.Environment.GetEnvironmentVariable("PLEXRIPPER_ROOT_PATH");
    }

    /// <summary>
    /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot PlexRipper in development mode in a non-docker environment.
    /// </summary>
    public static string? GetUserHomeDirectoryPath()
    {
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
    }

    #endregion

    #region Set

    public static void SetIntegrationTestMode(bool state = false)
    {
        System.Environment.SetEnvironmentVariable(IntegrationTestModeKey, state.ToString());
    }

    #endregion

    #endregion
}