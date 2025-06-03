using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Logging;

public static class LogLevelMapper
{
    public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information, // Default to Information for unrecognized levels
        };
    }
}
