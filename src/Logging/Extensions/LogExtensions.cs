using Serilog;
using Serilog.Events;

namespace Logging;

public static class LogExtensions
{
    public static LogEvent ToLogEvent(
        this ILogger logInstance,
        LogEventLevel logLevel,
        string messageTemplate,
        Exception exception = default,
        string className = default!,
        string memberName = default!,
        int sourceLineNumber = default!,
        params object?[]? propertyValues)
    {
        var dateTimeOffset = DateTimeOffset.Now;

        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        logInstance.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties);
        if (parsedTemplate is null)
        {
            logInstance.Error("LogExtensions.ToLogEvent() => Failed to parse {MessageTemplate} with {@PropertyValues}", messageTemplate, propertyValues);
            return new LogEvent(dateTimeOffset, LogEventLevel.Error, null, MessageTemplate.Empty, new List<LogEventProperty>());
        }

        var properties = boundProperties?.ToList() ?? new List<LogEventProperty>();
        properties.AddRange(new List<LogEventProperty>()
        {
            // This works when each file only has 1 class and is named the same
            new(LogConfig.ClassNamePropertyName, new ScalarValue(className)),
            new(LogConfig.MemberNamePropertyName, new ScalarValue(memberName)),
            new(LogConfig.LineNumberPropertyName, new ScalarValue(sourceLineNumber)),
        });

        return new LogEvent(dateTimeOffset, logLevel, exception, parsedTemplate, properties);
    }

    public static string ToLogString(this LogEvent logEvent)
    {
        using var writer = new StringWriter();
        LogConfig.TemplateTextFormatter.Format(logEvent, writer);
        return writer.ToString();
    }

    public static string GetClassName(this LogEvent logEvent)
    {
        if (logEvent.Properties.ContainsKey(LogConfig.ClassNamePropertyName))
        {
            using var writer = new StringWriter();
            logEvent.Properties[LogConfig.ClassNamePropertyName].Render(writer);
            return writer.ToString().Replace("\"", "");
        }

        return string.Empty;
    }

    public static string GetMethodName(this LogEvent logEvent)
    {
        if (logEvent.Properties.ContainsKey(LogConfig.MemberNamePropertyName))
        {
            using var writer = new StringWriter();
            logEvent.Properties[LogConfig.MemberNamePropertyName].Render(writer);
            return writer.ToString().Replace("\"", "");
        }

        return string.Empty;
    }
}