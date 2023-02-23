using Logging.Interface;
using Serilog;

namespace PlexRipper.BaseTests;

public class TestLoggingClass
{
    private readonly ILogger _logger;
    private readonly ILog<TestLoggingClass> _log;

    public TestLoggingClass(ILogger logger, ILog<TestLoggingClass> log)
    {
        _logger = logger;
        _log = log;
    }

    public void LogEvents()
    {
        // _logger.ForContext<TestLoggingClass>();
        // _logger.Verbose("LogTrace message");
        // _logger.Debug("LogDebug message");
        // _logger.Information("LogInformation message");
        // _logger.Error("LogError message");
        // _logger.Fatal("LogCritical message");
        var position = new { Latitude = 25, Longitude = 134 };
        var elapsedMs = 34;

        var string1 = "asfasf";
        var string2 = "asfasf";

        //_log.Debug("[DEBUG] - Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

        //_log.Debug(string.Format("[DEBUG LOL] - Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs), 0);
        // _log.Debug("[DEBUG] - Processed {String1} in {String2} ms.", string1, string2);
        _log.Debug("[DEBUG LINE] - LogDebug message: {Position}", position);

        _logger.Debug("[LOGGER DEBUG LINE] - LogDebug message: {Position}", position);

        //_log.Information("[INFORMATION] - Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        // _log.InformationLine("[INFORMATION LINE] - LogDebug message");
    }
}