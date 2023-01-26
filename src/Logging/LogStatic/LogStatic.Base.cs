#nullable enable
using Serilog;
using Serilog.Events;

namespace Logging.LogStatic;

public static partial class LogStatic
{
    public static bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return Log.IsEnabled(logLevel);
    }

    private static LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = Log.Logger.ToLogEvent(logLevel, messageTemplate, null, sourceFilePath, memberName, sourceLineNumber, propertyValues);

        Log.Write(logEvent);

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
        var logEvent = Log.Logger.ToLogEvent(logLevel, messageTemplate, exception, sourceFilePath, memberName, sourceLineNumber, propertyValues);

        Log.Write(logEvent);

        return logEvent;
    }

    private static void FatalAction()
    {
        System.Environment.ExitCode = -1;
    }
}