using Serilog.Events;

namespace Logging;

public static class LogExtensions
{
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