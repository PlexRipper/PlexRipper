using System;

namespace Environment
{
    public static class EnvironmentExtensions
    {
        #region Fields

        private static string _integrationTestModeKey = "IntegrationTestMode";

        private static readonly string _trueValue = Convert.ToString(true);

        #endregion

        #region Public Methods

        #region Get

        public static bool IsIntegrationTestMode()
        {
            return System.Environment.GetEnvironmentVariable(_integrationTestModeKey) == _trueValue;
        }

        #endregion

        #region Set

        public static void SetIntegrationTestMode(bool state = false)
        {
            System.Environment.SetEnvironmentVariable(_integrationTestModeKey, state.ToString());
        }

        #endregion

        #endregion
    }
}