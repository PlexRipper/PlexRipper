using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlexRipper.WebAPI.Common
{
    public static class PlexRipperHost
    {
        public static IHostBuilder Setup()
        {
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
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());
        }
    }
}