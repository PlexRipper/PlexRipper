using System.IO;
using FluentResults;
using PlexRipper.Application.Common;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Log = Serilog.Log;

namespace PlexRipper.FileSystem
{
    public class LogSystem : ILogSystem
    {
        private readonly IPathSystem _pathSystem;

        public static string Template = "{NewLine}{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

        public LogSystem(IPathSystem pathSystem)
        {
            _pathSystem = pathSystem;
        }

        public static LoggerConfiguration GetBaseConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Quartz", LogEventLevel.Information)
                .WriteTo.Debug(outputTemplate: Template, restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: Template);
        }

        public LoggerConfiguration GetLogFileConfiguration()
        {
            return GetBaseConfiguration()
                .WriteTo.File(
                    Path.Combine(_pathSystem.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    Template,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7);
        }

        public Logger GetLogger()
        {
            return GetLogFileConfiguration()
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        public Result Setup()
        {
            Log.Logger = GetLogger();
            Log.Information("Setting up Log System Service");
            return Result.Ok();
        }
    }
}