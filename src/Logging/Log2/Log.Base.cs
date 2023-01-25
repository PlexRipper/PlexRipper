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

    private LogEvent ToLogEvent(
        LogEventLevel logLevel,
        string messageTemplate,
        Exception? exception = default,
        string memberName = default!,
        string sourceFilePath = default!,
        int sourceLineNumber = default!,
        params object?[]? propertyValues)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _logger.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties);

        var dateTimeOffset = DateTimeOffset.Now;
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        var properties = boundProperties.ToList();
        properties.AddRange(new List<LogEventProperty>()
        {
            new("FileName", new ScalarValue(fileName)),
            new("MemberName", new ScalarValue(memberName)),
            new("LineNumber", new ScalarValue(sourceLineNumber)),
        });

        return new LogEvent(dateTimeOffset, logLevel, null, parsedTemplate, properties);
    }

    private LogEvent Write(
        LogEventLevel logLevel,
        string messageTemplate,
        string memberName = "",
        string sourceFilePath = "",
        int sourceLineNumber = 0,
        params object?[]? propertyValues)
    {
        var logEvent = ToLogEvent(logLevel, messageTemplate, null, memberName, sourceFilePath, sourceLineNumber, propertyValues);

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
        var logEvent = ToLogEvent(logLevel, messageTemplate, exception, memberName, sourceFilePath, sourceLineNumber, propertyValues);

        _logger.Write(logEvent);

        return logEvent;
    }
}