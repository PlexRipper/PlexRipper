// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

using Reaparr.Logging;

// ReSharper disable once CheckNamespace
namespace FluentResults;

public static partial class ResultExtensions
{
    private static ILog _log = new LogConfig().CreateLogInstance(typeof(ResultExtensions));

    public static void SetLogger(ILog log)
    {
        _log = log;
    }
}
