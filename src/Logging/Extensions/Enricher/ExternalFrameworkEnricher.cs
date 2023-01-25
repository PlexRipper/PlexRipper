using Serilog.Core;
using Serilog.Events;

namespace Logging.Enricher;

/// <summary>
///
/// </summary>
public class ExternalFrameworkEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var value))
        {
            var nameSpace = value.ToString();

            var parts = nameSpace.Split('.').Select(x => x.Trim('\"')).ToList();
            logEvent.AddPropertyIfAbsent(new LogEventProperty("FileName", new ScalarValue(parts[0])));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("MemberName", new ScalarValue(parts.Last())));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("LineNumber", new ScalarValue(0)));
        }
    }
}