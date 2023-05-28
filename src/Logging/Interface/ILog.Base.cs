using Serilog.Events;

namespace Logging.Interface;

public partial interface ILog
{
    bool IsLogLevelEnabled(LogEventLevel logLevel = LogEventLevel.Debug);
}