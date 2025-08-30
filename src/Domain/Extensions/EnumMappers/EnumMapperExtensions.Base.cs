using Logging.Interface;

namespace PlexRipper.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly ILog _log = new LogConfig().CreateLogInstance(typeof(EnumMapperExtensions));
}
