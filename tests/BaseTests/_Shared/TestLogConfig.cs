using Serilog.Core;
using Serilog.Events;

namespace Reaparr.BaseTests;

public class TestLogConfig : LogConfig
{
    private readonly ITestOutputHelper? _testOutput;

    public TestLogConfig(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        if (_testOutput is not null)
        {
            // Test Logger
            return GetBaseConfiguration()
                .WriteTo.TestOutput(_testOutput, TemplateTextFormatter, minimumLogLevel)
                .WriteTo.TestCorrelator(minimumLogLevel)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
        }

        return base.GetLogger(minimumLogLevel);
    }
}
