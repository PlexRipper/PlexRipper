using System;

namespace Environment
{
    public static class EnvironmentExtensions
    {
        #region Fields

        public static string IntegrationTestModeKey = "IntegrationTestMode";

        private static readonly string _trueValue = Convert.ToString(true);

        #endregion

        #region Public Methods

        #region Get

        public static bool IsIntegrationTestMode()
        {
            return System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == _trueValue;
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