using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PlexRipper.Domain
{
    public static class LogConfigurationExtensions
    {

        public static string Template = "{NewLine}{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";


        public static LoggerConfiguration GetBaseConfiguration =>

            new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                // .WriteTo.Console(outputTemplate: Template)
                .WriteTo.Debug(outputTemplate: Template) // Print logging to the debug window
                .WriteTo.ColoredConsole(outputTemplate: Template);

        public static Logger GetLogger()
        {
            return GetBaseConfiguration
                    .MinimumLevel.Debug()
                    .CreateLogger();
        }
    }
}
