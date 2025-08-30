using Reaparr.Logging;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.WebAPI;

public static partial class Startup
{
    private static readonly ILog _log = new LogConfig().CreateLogInstance(typeof(Startup));

    /// <summary>
    ///  The CORS Configuration name.
    /// </summary>
    public const string CORSConfiguration = "CORS_Configuration";
}
