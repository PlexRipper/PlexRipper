using Serilog.Events;
using Serilog.Formatting;

namespace Reaparr.Logging;

public class ConditionalTextFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        output.Write($"{System.Environment.NewLine}{logEvent.Timestamp:HH:mm:ss} [{logEvent.Level}] ");

        var fileName = logEvent.GetStringProperty(LogConfig.FileName);
        var lineNumber = logEvent.GetIntProperty(LogConfig.LineNumber);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            output.Write($"[{fileName}:{lineNumber}");
            var methodName = logEvent.GetStringProperty(LogConfig.MethodName);
            output.Write($".{methodName}()]");
        }
        else
        {
            var sourceContext = logEvent.GetStringProperty(LogConfig.SourceContext);
            output.Write($"[{sourceContext}]");
        }

        output.Write(" => ");
        output.Write(logEvent.RenderMessage());

        if (logEvent.Exception != null)
            output.WriteLine(logEvent.Exception);
    }
}
