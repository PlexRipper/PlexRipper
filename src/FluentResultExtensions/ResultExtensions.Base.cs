// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

using Reaparr.Logging;
using Serilog;

// ReSharper disable once CheckNamespace
namespace FluentResults;

public static partial class ResultExtensions
{
    private static ILogger _log = new LogConfig().CreateLogInstance(typeof(ResultExtensions));

    public static void SetLogger(ILogger log)
    {
        _log = log.ForContext(typeof(ResultExtensions));
    }
}
