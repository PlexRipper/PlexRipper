using Serilog;
using Serilog.Events;

namespace Reaparr.Logging;

public record LogMetaData
{
    public LogMetaData(ILogger log, string className, string memberName, int lineNumber)
    {
        _logger = log;
    }

    private ILogger _logger { get; } = null!;

    public string MessageTemplate { get; set; } = string.Empty;
    public object?[]? PropertyValues { get; set; }
    public LogEventLevel LogLevel { get; set; }
    public Exception? Exception { get; set; }

    public LogMetaData Update(LogEventLevel logLevel, string messageTemplate, params object?[]? propertyValues)
    {
        LogLevel = logLevel;
        MessageTemplate = messageTemplate;
        PropertyValues = propertyValues;
        return this;
    }

    /// <summary>
    /// Returns a rendered string of the message template with bound properties.
    /// </summary>
    public override string ToString() => ToEvent().RenderMessage();

    private LogEvent ToEvent()
    {
        var dateTimeOffset = DateTimeOffset.Now;

        _logger.BindMessageTemplate(MessageTemplate, PropertyValues, out var parsedTemplate, out var boundProperties);
        if (parsedTemplate is null)
        {
            _logger.Error(
                "LogExtensions.ToLogEvent() => Failed to parse {MessageTemplate} with {@PropertyValues}",
                MessageTemplate,
                PropertyValues
            );
            return new LogEvent(
                dateTimeOffset,
                LogEventLevel.Error,
                null,
                Serilog.Events.MessageTemplate.Empty,
                new List<LogEventProperty>()
            );
        }

        var properties = boundProperties?.ToList() ?? new List<LogEventProperty>();

        return new LogEvent(dateTimeOffset, LogLevel, Exception, parsedTemplate, properties);
    }
}
