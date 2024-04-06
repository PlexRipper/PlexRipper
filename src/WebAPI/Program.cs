using Environment;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.WebAPI;

public class Program
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Program));

    public static void Main(string[] args)
    {
        try
        {
            LogManager.SetupLogging(LogEventLevel.Verbose);
            _log.Information("Currently running on {CurrentOS}", OsInfo.CurrentOS);

            _log.Information("Root directory: {RootDirectory}", PathProvider.RootDirectory);
            _log.InformationLine("The Root directory can be set by passing in the \'PLEXRIPPER_ROOT_PATH\' env value with a valid folder path");

            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureDatabase();

            builder.Host.ConfigureHostBuilder();

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
}