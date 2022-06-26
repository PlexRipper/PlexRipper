#nullable enable
namespace Environment
{
    public static class EnvironmentExtensions
    {
        #region Fields

        public static string IntegrationTestModeKey = "IntegrationTestMode";

        public static string DevelopmentRootPathKey = "DEVELOPMENT_ROOT_PATH";

        private static readonly string _trueValue = Convert.ToString(true);

        #endregion

        #region Public Methods

        #region Get

        public static bool IsIntegrationTestMode()
        {
            return System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == _trueValue;
        }

        /// <summary>
        /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot PlexRipper in development mode in a non-docker environment.
        /// </summary>
        /// <returns></returns>
        public static string? GetDevelopmentRootPath()
        {
            return System.Environment.GetEnvironmentVariable(DevelopmentRootPathKey);
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
}