// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package

using Logging;
using Logging.Interface;

namespace FluentResults;

public static partial class ResultExtensions
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(ResultExtensions));

    public static void SetLogger(ILog log)
    {
        _log = log;
    }
}
