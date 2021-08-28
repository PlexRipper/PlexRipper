using System;
using System.IO;
using System.Runtime.CompilerServices;
using Environment;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit.Abstractions;

namespace Logging
{
    public static class Log
    {
        private static string FormatForException(this string message, Exception ex)
        {
            return $"{message}: {ex?.ToString() ?? string.Empty}";
        }

        private static string FormatForContext(
            this string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

            return $"[{fileName}.{memberName}] => {message}";
        }

        public static string Template => "{NewLine}{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

        public static LoggerConfiguration GetBaseConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Quartz", LogEventLevel.Information)
                .WriteTo.Debug(outputTemplate: Template, restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: Template)
                .WriteTo.File(
                    Path.Combine(PathSystem.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    Template,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7);;
        }

        #region Setup

        public static void SetupLogging()
        {
            Serilog.Log.Logger =
                GetBaseConfiguration()
                    .MinimumLevel.Debug()
                    .CreateLogger();
        }

        public static void SetupTestLogging(ITestOutputHelper output)
        {
            Serilog.Log.Logger =
                GetBaseConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.TestOutput(output, outputTemplate: Template)
                    .WriteTo.TestCorrelator()
                    .CreateLogger();
        }

        #endregion

        #region Verbose

        public static void Verbose(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var messageTemplate = message.FormatForContext(memberName, sourceFilePath);
            Serilog.Log.Verbose("{MessageTemplate}", messageTemplate);
        }

        public static void Verbose(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var messageTemplate = message.FormatForException(ex).FormatForContext(memberName, sourceFilePath);
            Serilog.Log.Verbose("{MessageTemplate}", messageTemplate);
        }

        public static void Verbose(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            var messageTemplate = (ex?.ToString() ?? string.Empty).FormatForContext(memberName, sourceFilePath);
            Serilog.Log.Verbose("{MessageTemplate}", messageTemplate);
        }

        #endregion

        #region Debug

        public static void Debug(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Debug(message.FormatForContext(memberName, sourceFilePath));
        }

        public static void Debug(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Debug(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Debug(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Debug(
                (ex != null ? ex.ToString() : string.Empty)
                .FormatForContext(memberName, sourceFilePath)
            );
        }

        #endregion

        #region Information

        public static void Information(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Information(
                message
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Information(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Information(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Information(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Information(
                (ex != null ? ex.ToString() : string.Empty)
                .FormatForContext(memberName, sourceFilePath)
            );
        }

        #endregion

        #region Warning

        public static void Warning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Warning(
                message
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Warning(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Warning(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Warning(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Warning(
                (ex != null ? ex.ToString() : string.Empty)
                .FormatForContext(memberName, sourceFilePath)
            );
        }

        #endregion

        #region Error

        public static void Error(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Error(
                message
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Error(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Error(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath)
            );
        }

        public static void Error(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            Serilog.Log.Error(
                (ex != null ? ex.ToString() : string.Empty)
                .FormatForContext(memberName, sourceFilePath)
            );
        }

        #endregion

        #region Fatal

        public static void Fatal(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Fatal(message.FormatForContext(memberName, sourceFilePath));
        }

        public static void Fatal(
            string message,
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Fatal(message.FormatForException(ex).FormatForContext(memberName, sourceFilePath));
        }

        public static void Fatal(
            Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            FatalAction();

            Serilog.Log.Fatal((ex?.ToString() ?? string.Empty).FormatForContext(memberName, sourceFilePath));
        }

        private static void FatalAction()
        {
            System.Environment.ExitCode = -1;
        }

        #endregion

        public static void CloseAndFlush()
        {
            Serilog.Log.CloseAndFlush();
        }
    }
}