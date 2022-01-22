using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.WebAPI.Common
{
    public static class PlexRipperHost
    {
        public static IHostBuilder Setup()
        {
            Log.Information("Starting up");
            Log.Information($"Currently running on {OsInfo.CurrentOS}");

            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>();
                })
                .ConfigureLogging(config =>
                {
                    config.ClearProviders();
                    config.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning);
                })
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    Log.Debug("Setting up Autofac Containers");
                    ContainerConfig.ConfigureContainer(containerBuilder);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());
        }
    }
}