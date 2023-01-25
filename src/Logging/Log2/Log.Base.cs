#nullable enable
using Logging.Interface;
using Serilog;
using Serilog.Events;

namespace Logging.Log2;

public partial class Log : ILog
{
    private readonly ILogger _logger;

    public Log(ILogger logger)
    {
        _logger = logger;
    }

    private LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = LogConfig.ToLogEvent(logLevel, messageTemplate, null, memberName, sourceFilePath, sourceLineNumber, propertyValues);

        _logger.Write(logEvent);

        return logEvent;
    }

    private LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception? exception = default,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = LogConfig.ToLogEvent(logLevel, messageTemplate, exception, memberName, sourceFilePath, sourceLineNumber, propertyValues);

        _logger.Write(logEvent);

        return logEvent;
    }
}