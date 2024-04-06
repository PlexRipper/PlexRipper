#nullable enable
namespace Environment;

public static class EnvironmentExtensions
{
    #region Fields

    public const string IntegrationTestModeKey = "IntegrationTestMode";

    public const string PlexRipperRootPathKey = "PLEXRIPPER_ROOT_PATH";

    private static readonly string TrueValue = Convert.ToString(true);

    #endregion

    #region Public Methods

    #region Get

    public static bool IsIntegrationTestMode() => System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == TrueValue;

    /// <summary>
    /// When a PlexRipperRootPath is set, it will be used as the root directory for the application.
    /// </summary>
    public static string? GetPlexRipperRootPathEnv() => System.Environment.GetEnvironmentVariable(PlexRipperRootPathKey);

    /// <summary>
    ///  Returns the path to the application directory bases on the current OS.
    /// </summary>
    public static string GetApplicationDirectoryPath() => System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

    #endregion

    #region Set

    public static void SetIntegrationTestMode(bool state = false)
    {
        System.Environment.SetEnvironmentVariable(IntegrationTestModeKey, state.ToString());
    }

    #endregion

    #endregion
}