using PlexRipper.Domain;
using Serilog;
using Xunit.Abstractions;
using Log = Serilog.Log;

namespace PlexRipper.BaseTests
{
    public static class BaseDependanciesTest
    {
        static ITestOutputHelper Output;

        public static ILogger GetLogger<T>()
        {
            return GetLoggerConfig().ForContext<T>();
        }

        public static void SetupLogging(ITestOutputHelper output)
        {
            Output = output;
            Log.Logger = GetLoggerConfig();
        }

        public static ILogger GetLoggerConfig()
        {
            var config = LogConfigurationExtensions.GetBaseConfiguration;
            return config
                .WriteTo.TestOutput(Output, outputTemplate: LogConfigurationExtensions.Template)
                .CreateLogger();
        }
    }
}