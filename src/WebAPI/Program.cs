using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using Logging.Interface;
using Serilog;
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

            var builder = WebApplication.CreateBuilder(args);

            var startup = new Startup();

            Startup.ConfigureDatabase();

            // Use Autofac as the DI container
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                _log.DebugLine("Setting up Autofac Containers");
                ContainerConfig.ConfigureContainer(containerBuilder);
            });

            // Add services to the container.
            builder.Host.UseSerilog(LogConfig.GetLogger());

            // Setup the services
            startup.ConfigureServices(builder.Services, builder.Environment);
            var app = builder.Build();

            // Setup the app
            startup.Configure(app, app.Environment);

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