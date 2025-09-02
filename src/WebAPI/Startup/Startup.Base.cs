namespace Reaparr.WebAPI;

public static partial class Startup
{
    private static readonly Serilog.ILogger _log = new LogConfig().CreateLogInstance(typeof(Startup));

    /// <summary>
    ///  The CORS Configuration name.
    /// </summary>
    public const string CORSConfiguration = "CORS_Configuration";
}
