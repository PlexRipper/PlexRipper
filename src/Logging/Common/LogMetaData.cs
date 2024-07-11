using Logging.Interface;
using Serilog;
using Serilog.Events;

namespace Logging.Common;

public record LogMetaData
{
    #region Constructors

    public LogMetaData(string className, string memberName, int lineNumber)
    {
        ClassName = className;
        MethodName = memberName;
        LineNumber = lineNumber;
    }

    public LogMetaData(ILog logger, string className, string memberName, int lineNumber)
    {
        _logger = logger.GetLogger();
        ClassName = className;
        MethodName = memberName;
        LineNumber = lineNumber;
    }

    #endregion

    #region Properties

    private ILogger _logger { get; } = null!;

    public string ClassName { get; init; }
    public string MethodName { get; init; }
    public int LineNumber { get; init; }

    public string MessageTemplate { get; set; } = string.Empty;
    public object?[]? PropertyValues { get; set; }
    public LogEventLevel LogLevel { get; set; }
    public Exception? Exception { get; set; }

    #endregion

    #region Methods

    #region Public

    public LogMetaData Update(LogEventLevel logLevel, string messageTemplate, params object?[]? propertyValues)
    {
        LogLevel = logLevel;
        MessageTemplate = messageTemplate;
        PropertyValues = propertyValues;
        return this;
    }

    public void Write()
    {
        _logger.Write(ToEvent());
    }

    public string ToLogString()
    {
        using var writer = new StringWriter();
        LogConfig.TemplateTextFormatter.Format(ToEvent(), writer);
        return writer.ToString();
    }

    #endregion

    #region Private

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

        properties.AddRange(
            new List<LogEventProperty>()
            {
                // This works when each file only has 1 class and is named the same
                new(nameof(ClassName), new ScalarValue(ClassName)),
                new(nameof(MethodName), new ScalarValue(MethodName)),
                new(nameof(LineNumber), new ScalarValue(LineNumber)),
            }
        );

        return new LogEvent(dateTimeOffset, LogLevel, Exception, parsedTemplate, properties);
    }

    #endregion

    #endregion
}
