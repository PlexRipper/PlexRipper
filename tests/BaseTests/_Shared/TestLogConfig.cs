using Serilog.Core;

namespace Reaparr.BaseTests;

public class TestLogConfig : LogConfig
{
    public TestLogConfig(IAppRuntimeInfo appRuntimeInfo, IPathProvider pathProvider)
        : base(appRuntimeInfo, pathProvider) { }

    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetBaseConfiguration(minimumLogLevel).WriteTo.TestCorrelator(minimumLogLevel).CreateLogger();
}
