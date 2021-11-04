using System;

namespace Environment
{
    public static class EnvironmentExtensions
    {
        #region Fields

        private static string _integrationTestModeKey = "IntegrationTestMode";

        private static string _memoryDbKey = "MemoryDB";

        private static string _resetDbKey = "ResetDB";

        private static readonly string _trueValue = Convert.ToString(true);

        #endregion

        #region Public Methods

        #region Get

        public static bool IsInMemoryDatabase()
        {
            return System.Environment.GetEnvironmentVariable(_memoryDbKey) == _trueValue;
        }

        public static bool IsIntegrationTestMode()
        {
            return System.Environment.GetEnvironmentVariable(_integrationTestModeKey) == _trueValue;
        }

        public static bool IsResetDatabase()
        {
            return System.Environment.GetEnvironmentVariable(_resetDbKey) == _trueValue;
        }

        #endregion

        #region Set

        public static void SetInMemoryDatabase(bool state = true)
        {
            System.Environment.SetEnvironmentVariable(_memoryDbKey, state.ToString());
        }

        public static void SetIntegrationTestMode(bool state = false)
        {
            System.Environment.SetEnvironmentVariable(_integrationTestModeKey, state.ToString());
        }

        public static void SetResetDatabase(bool state = false)
        {
            System.Environment.SetEnvironmentVariable(_resetDbKey, state.ToString());
        }

        #endregion

        #endregion
    }
}