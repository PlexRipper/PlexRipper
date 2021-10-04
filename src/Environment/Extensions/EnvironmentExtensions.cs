using System;

namespace Environment
{
    public static class EnvironmentExtensions
    {
        private static string _integrationTestModeKey = "IntegrationTestMode";

        private static string _resetDbKey = "ResetDB";

        private static string _memoryDbKey = "ResetDB";

        public static bool IsIntegrationTestMode()
        {
            return System.Environment.GetEnvironmentVariable(_integrationTestModeKey) is "true";
        }

        public static void SetIntegrationTestMode()
        {
            System.Environment.SetEnvironmentVariable(_integrationTestModeKey, "true");
        }

        public static bool IsResetDatabase()
        {
            return System.Environment.GetEnvironmentVariable(_resetDbKey) is "true";
        }

        public static void SetResetDatabase()
        {
            System.Environment.SetEnvironmentVariable(_resetDbKey, "true");
        }

        public static void SetInMemoryDatabase(bool state = true)
        {
            System.Environment.SetEnvironmentVariable(_memoryDbKey, state.ToString());
        }
    }
}