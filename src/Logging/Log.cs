using System.Runtime.CompilerServices;
using Environment;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit.Abstractions;

namespace Logging;

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

    private static string Template => "{NewLine}{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

    private static LoggerConfiguration GetBaseConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Information)
            .WriteTo.Debug(outputTemplate: Template)
            .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: Template)
            .WriteTo.File(
                Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                LogEventLevel.Debug,
                Template,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7);
    }

    #region Setup

    public static void SetupLogging(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        Serilog.Log.Logger =
            GetBaseConfiguration()
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
    }

    public static void SetupTestLogging(ITestOutputHelper output, LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        Serilog.Log.Logger =
            GetBaseConfiguration()
                .MinimumLevel.Is(minimumLogLevel)
                .WriteTo.TestOutput(output, minimumLogLevel, Template)
                .WriteTo.TestCorrelator(minimumLogLevel)
                .CreateLogger();
    }

    #endregion

    public static void DbContextLogger(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        switch (message)
        {
            // ReSharper disable once StringLiteralTypo
            case { } s when s.StartsWith("dbug:"):
                Debug(message, memberName, sourceFilePath);
                break;
            case { } s when s.StartsWith("info:"):
                Information(message, memberName, sourceFilePath);
                break;
            case { } s when s.StartsWith("fail:"):
                Error(message, memberName, sourceFilePath);
                break;
        }
    }

    #region Verbose

    public static void Verbose(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(message.FormatForContext(memberName, sourceFilePath));
    }

    public static void Verbose(
        string message,
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(
            message
                .FormatForException(ex)
                .FormatForContext(memberName, sourceFilePath));
    }

    public static void Verbose(
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(
            (ex != null ? ex.ToString() : string.Empty)
            .FormatForContext(memberName, sourceFilePath)
        );
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

    public static void Warning(
        string message,
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