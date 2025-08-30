using Serilog.Events;

namespace Reaparr.Logging;

public partial interface ILog
{
    bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug);

    bool IsLogLevelVerbose();

    bool IsLogLevelDebug();
}
