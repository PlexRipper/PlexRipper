namespace Reaparr.AppHost;

public static partial class Startup
{
    private static readonly Serilog.ILogger _log = LogFactory.Create(typeof(Startup));

    /// <summary>
    ///  The CORS Configuration name.
    /// </summary>
    public const string CorsConfiguration = "CORS_Configuration";
}
