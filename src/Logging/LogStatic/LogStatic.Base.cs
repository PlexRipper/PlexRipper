#nullable enable
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    public static bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return Serilog.Log.IsEnabled(logLevel);
    }

    private static LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = LogConfig.ToLogEvent(logLevel, messageTemplate, null, memberName, sourceFilePath, sourceLineNumber, propertyValues);

        Serilog.Log.Write(logEvent);

        return logEvent;
    }

    private static LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception? exception = default,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = LogConfig.ToLogEvent(logLevel, messageTemplate, exception, memberName, sourceFilePath, sourceLineNumber, propertyValues);

        Serilog.Log.Write(logEvent);

        return logEvent;
    }

    private static void FatalAction()
    {
        System.Environment.ExitCode = -1;
    }
}