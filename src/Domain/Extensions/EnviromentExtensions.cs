using System;

namespace PlexRipper.Domain
{
    public static class EnviromentExtensions
    {
        private static string _integrationTestModeKey = "IntegrationTestMode";

        private static string _resetDbKey = "ResetDB";

        public static bool IsIntegrationTestMode()
        {
            return Environment.GetEnvironmentVariable(_integrationTestModeKey) is "true";
        }

        public static void SetIntegrationTestMode()
        {
            Environment.SetEnvironmentVariable(_integrationTestModeKey, "true");
        }

        public static bool IsResetDatabase()
        {
            return Environment.GetEnvironmentVariable(_resetDbKey) is "true";
        }

        public static void SetResetDatabase()
        {
            Environment.SetEnvironmentVariable(_resetDbKey, "true");
        }
    }
}