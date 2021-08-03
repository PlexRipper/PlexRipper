using System;

namespace PlexRipper.Domain
{
    public static class EnviromentExtensions
    {
        public static bool IsIntegrationTestMode()
        {
            return Environment.GetEnvironmentVariable("IntegrationTestMode") is "true";
        }

        public static void SetIntegrationTestMode()
        {
            Environment.SetEnvironmentVariable("IntegrationTestMode", "true");
        }

        public static bool IsResetDatabase()
        {
            return Environment.GetEnvironmentVariable("ResetDB") is "true";
        }

        public static void SetResetDatabase()
        {
            Environment.SetEnvironmentVariable("ResetDB", "true");
        }
    }
}