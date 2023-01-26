using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace Logging.UnitTests.LogTheme;

public class LogTheme_UnitTests
{
    private readonly ITestOutputHelper _output;

    public LogTheme_UnitTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_When()
    {
        var position = new { Latitude = 25, Longitude = 134 };
        var logLevels = new List<LogEventLevel>()
        {
            LogEventLevel.Verbose,
            LogEventLevel.Debug,
            LogEventLevel.Information,
            LogEventLevel.Warning,
            LogEventLevel.Error,
            LogEventLevel.Fatal,
        };

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        for (var i = 0; i < 1000; i++)
        {
            var theme = FakeData.GetFakeTheme(i).Generate();
            using var logger = new LoggerConfiguration()
                .WriteTo.TestOutput(_output, LogConfig.TemplateTextFormatter)
                .WriteTo.Debug(outputTemplate: LogConfig.Template)
                .WriteTo.Console(theme: theme, outputTemplate: LogConfig.Template)
                .CreateLogger();

            foreach (var logEventLevel in logLevels)
            {
                var logEvent = logger.ToLogEvent(logEventLevel,
                    "This is a {LogEventLevel} log message with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                    null,
                    "TestClass",
                    "TestMethod",
                    666,
                    // template properties
                    Enum.GetName(logEventLevel),
                    position,
                    9999,
                    true);
                logger.Write(logEvent);
            }
        }

        stopWatch.Stop();
        _output.WriteLine($"Elapsed time: {stopWatch.ElapsedMilliseconds}");
    }
}