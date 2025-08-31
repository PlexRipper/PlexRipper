using Reaparr.Logging;
using Serilog;

namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly ILogger _log = new LogConfig().CreateLogInstance(typeof(EnumMapperExtensions));
}
