using Logging.Interface;
using Serilog;

namespace PlexRipper.BaseTests;

public class TestLoggingClass
{
    private readonly ILogger _logger;
    private readonly ILog _log;

    public TestLoggingClass(ILogger logger, ILog log)
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

        _log.Debug("[DEBUG] - Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        _log.DebugLine("[DEBUG LINE] - LogDebug message");
        _log.Information("[INFORMATION] - Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        _log.InformationLine("[INFORMATION LINE] - LogDebug message");
    }
}