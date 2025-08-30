using Serilog.Events;

namespace Reaparr.Logging;

public interface ILog
{
    bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug);

    bool IsLogLevelVerbose();

    bool IsLogLevelDebug();
}
