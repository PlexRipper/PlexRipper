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
            if (parts.Count < 3)
                return;

            logEvent.AddPropertyIfAbsent(new LogEventProperty(LogConfig.ClassNamePropertyName, new ScalarValue(parts[0])));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(LogConfig.MemberNamePropertyName, new ScalarValue(parts.Last())));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(LogConfig.LineNumberPropertyName, new ScalarValue(0)));
        }
    }
}