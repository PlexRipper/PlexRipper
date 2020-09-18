using PlexRipper.Domain;
using Serilog;
using Xunit.Abstractions;

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
            Serilog.Log.Logger = GetLoggerConfig();
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