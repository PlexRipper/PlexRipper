using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public static class BaseDependanciesTest
    {
        static ITestOutputHelper Output;

        private static readonly string _template = "{NewLine}{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

        public static void SetupLogging(ITestOutputHelper output)
        {
            Output = output;
            Log.Logger = GetLoggerConfig();
        }

        public static ILogger GetLoggerConfig()
        {
            // A separate config variable is needed here for some reason
            return new LoggerConfiguration()
                .MinimumLevel.Verbose().WriteTo.Debug(outputTemplate: _template, restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: _template)
                .WriteTo.TestOutput(Output, outputTemplate: _template)
                .WriteTo.TestCorrelator()
                .CreateLogger();
        }
    }
}