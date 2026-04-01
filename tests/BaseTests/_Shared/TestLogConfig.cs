using Serilog.Core;

namespace Reaparr.BaseTests;

public class TestLogConfig : LogConfig
{
    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration().WriteTo.TestCorrelator(minimumLogLevel).MinimumLevel.Is(minimumLogLevel).CreateLogger();
}
