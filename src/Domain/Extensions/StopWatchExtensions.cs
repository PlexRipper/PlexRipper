using System.Diagnostics;
using Logging.Interface;

namespace PlexRipper.Domain;

public static class StopWatchExtensions
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(StopWatchExtensions));

    public static void StopAndLog(this Stopwatch stopwatch, string prefix = "")
    {
        stopwatch.Stop();
        if (prefix.Length > 0)
        {
            _log.Information(
                "{Prefix} - Execution Time: took {ElapsedMilliseconds} ms",
                prefix,
                stopwatch.ElapsedMilliseconds
            );
            return;
        }

        _log.Information("Execution Time: took {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }
}
