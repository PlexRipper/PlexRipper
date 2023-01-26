using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using Logging.Interface;
using PlexRipper.Data;
using Serilog;
using Serilog.Events;

namespace PlexRipper.WebAPI.Common;

public static class PlexRipperHost
{
    private static readonly ILog _log = LogConfig.GetLog(typeof(PlexRipperHost));

    public static IHostBuilder Setup()
    {
        LogConfig.SetupLogging(LogEventLevel.Verbose);
        _log.Information("Currently running on {CurrentOS}", OsInfo.CurrentOS);

        return Host.CreateDefaultBuilder()
            .UseSerilog(LogConfig.GetLogger())
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>();
            })
            .ConfigureDatabase()
            .ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                _log.DebugLine("Setting up Autofac Containers");
                ContainerConfig.ConfigureContainer(containerBuilder);
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }

    public static IHostBuilder ConfigureDatabase(this IHostBuilder hostBuilder)
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
            return hostBuilder;

        var dbContext = new PlexRipperDbContext(new PathProvider());
        dbContext.Setup();

        return hostBuilder;
    }
}