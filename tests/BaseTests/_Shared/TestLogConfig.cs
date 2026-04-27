using Serilog.Core;

namespace Reaparr.BaseTests;

public class TestLogConfig : LogConfig
{
    public TestLogConfig(IPathProvider pathProvider)
        : base(pathProvider) { }

    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration(minimumLogLevel).WriteTo.TestCorrelator(minimumLogLevel).CreateLogger();
}
