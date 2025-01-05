using Logging.Interface;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Startup));

    /// <summary>
    ///  The CORS Configuration name.
    /// </summary>
    public const string CORSConfiguration = "CORS_Configuration";
}
