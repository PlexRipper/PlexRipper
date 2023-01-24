using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using PlexRipper.Data;
using Serilog;
using Serilog.Events;
using Log = Logging.Log;

namespace PlexRipper.WebAPI.Common;

public static class PlexRipperHost
{
    public static IHostBuilder Setup()
    {
        LogConfig.SetupLogging(LogEventLevel.Verbose);

        Log.Information("Starting up");
        Log.Information($"Currently running on {OsInfo.CurrentOS}");

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
                Log.Debug("Setting up Autofac Containers");
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