using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;

namespace Reaparr.BaseTests;

public static class TestOutputLoggerConfigurationExtensions
{
    public static LoggerConfiguration TestOutput(
        this LoggerSinkConfiguration sinkConfiguration,
        ITestOutputHelper testOutputHelper,
        ITextFormatter formatter,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum
    ) => sinkConfiguration.Sink(new TestOutputSink(testOutputHelper, formatter), restrictedToMinimumLevel);
}
