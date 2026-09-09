using Serilog;

namespace Reaparr.FluentResultExtensions;

public static partial class ResultExtensions
{
    private static readonly ILogger _log = LogFactory.Create(typeof(ResultExtensions));
}
