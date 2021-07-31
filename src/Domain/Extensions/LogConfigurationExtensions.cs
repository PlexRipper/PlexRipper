using System.IO;
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
                .MinimumLevel.Override("Quartz", LogEventLevel.Information)
                .WriteTo.Debug(outputTemplate: Template, restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.ColoredConsole(outputTemplate: Template)
                .WriteTo.File(
                    Path.Combine(FileSystemPaths.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    Template,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7);

        public static Logger GetLogger()
        {
            return GetBaseConfiguration
                .MinimumLevel.Debug()
                .CreateLogger();
        }
    }
}