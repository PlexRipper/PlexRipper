#nullable enable
using Logging.Interface;
using Serilog;
using Serilog.Events;

namespace Logging;

public partial class Log : ILog
{
    private readonly ILogger _logger;

    protected Type ClassType;

    public Log(ILogger logger)
    {
        _logger = logger;
    }

    public bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return _logger.IsEnabled(logLevel);
    }

    private string GetClassName(string sourceFilePath)
    {
        if (ClassType?.FullName != null)
        {
            var parts = ClassType.FullName.Split('.').Select(x => x.Trim('\"')).ToList();
            if (parts.Any())
                return parts.Last();
        }

        return Path.GetFileNameWithoutExtension(sourceFilePath);
    }

    private LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = _logger.ToLogEvent(logLevel, messageTemplate, null, GetClassName(sourceFilePath), memberName, sourceLineNumber, propertyValues);

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
        var logEvent = _logger.ToLogEvent(logLevel, messageTemplate, exception, GetClassName(sourceFilePath), memberName, sourceLineNumber, propertyValues);

        _logger.Write(logEvent);

        return logEvent;
    }
}