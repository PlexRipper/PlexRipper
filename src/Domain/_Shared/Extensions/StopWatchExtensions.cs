using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Reaparr.Domain;

public static class StopWatchExtensions
{
    private static readonly ILogger _log = LogFactory.GetLogger(typeof(StopWatchExtensions));

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

        _log.Here().Debug("Execution Time: took {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }
}
