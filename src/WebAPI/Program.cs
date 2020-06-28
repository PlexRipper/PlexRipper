using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Serilog;
using System;
using System.IO;
using Log = PlexRipper.Domain.Log;

namespace PlexRipper.WebAPI
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Serilog.Log.Logger = LogConfigurationExtensions.GetLogger();

            try
            {
                Log.Information("Starting up");

                var host = Host.CreateDefaultBuilder(args)
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
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
                    .ConfigureServices((hostContext, services) => { services.AddHostedService<DownloadWorker>(); })
                    .UseSerilog()
                    .Build();

                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }
    }
}
