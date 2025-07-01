using System.Diagnostics;
using System.Runtime.CompilerServices;
using Logging.Interface;

namespace PlexRipper.Domain;

public static class StopWatchExtensions
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(StopWatchExtensions));

    public static void StopAndLog(
        this Stopwatch stopwatch,
        string prefix = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        stopwatch.Stop();
        if (prefix.Length > 0)
        {
            _log.Here(sourceFilePath, memberName, sourceLineNumber)
                .Debug(
                    "{Prefix} - Execution Time: took {ElapsedMilliseconds} ms",
                    prefix,
                    stopwatch.ElapsedMilliseconds
                );
            return;
        }

        _log.Debug("Execution Time: took {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }
}
