using Serilog.Core;
using Serilog.Events;

namespace Reaparr.BaseTests;

public class TestLogConfig : LogConfig
{
    private readonly ITestOutputHelper? _testOutput;
    private readonly bool _useTUnit;

    public TestLogConfig(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    public TestLogConfig()
    {
        _useTUnit = true;
    }

    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        if (_useTUnit)
            return GetBaseConfiguration()
                .WriteTo.TUnitOutput(restrictedToMinimumLevel: minimumLogLevel)
                .WriteTo.TestCorrelator(minimumLogLevel)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();

        if (_testOutput is not null)
            return GetBaseConfiguration()
                .WriteTo.TestOutput(_testOutput, TemplateTextFormatter, minimumLogLevel)
                .WriteTo.TestCorrelator(minimumLogLevel)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();

        return base.GetLogger(minimumLogLevel);
    }
}
