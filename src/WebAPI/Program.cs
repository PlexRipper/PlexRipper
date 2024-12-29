using System.Runtime.InteropServices;
using Environment;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.WebAPI;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    /// <summary>
    ///  Get the real user ID of the calling process.
    /// </summary>
    /// <returns></returns>
    [DllImport("libc")]
    public static extern uint getuid();

    /// <summary>
    ///  Get the real group ID of the calling process.
    /// </summary>
    /// <returns></returns>
    [DllImport("libc")]
    public static extern uint getgid();

    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Program));

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            LogManager.SetupLogging(EnvironmentExtensions.GetLogLevel());

            var version = EnvironmentExtensions.GetVersion();
            _log.Information(
                "Currently running {Channel} version {Version} on {CurrentOS}",
                version.Contains("dev") ? "DEVELOPMENT" : "STABLE",
                version,
                OsInfo.CurrentOS
            );

            LogIdentity();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAutofacBuilder();

            builder.Services.ConfigureServices(builder.Environment);

            var app = builder.Build();

            app.ConfigureApplication(app.Environment);

            app.Run();
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogFatal();
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.CloseAndFlush();
        }
    }

    private static void LogIdentity()
    {
        // Retrieve PUID and PGID from environment variables
        var puid = System.Environment.GetEnvironmentVariable("PUID");
        var pgid = System.Environment.GetEnvironmentVariable("PGID");

        _log.Debug("PUID from env: {PUID} and from the system: {PUID}", puid ?? "-1", getuid());
        _log.Debug("PGID from env: {PGID} and from the system: {PGID}", pgid ?? "-1", getgid());
        _log.Debug("Current system Username: {SystemPUIDName}", System.Environment.UserName);
    }
}
