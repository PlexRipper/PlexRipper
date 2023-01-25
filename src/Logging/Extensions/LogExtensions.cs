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
}