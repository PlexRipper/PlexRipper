using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;
using Serilog;
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
                Log.Information($"Currently running on {OsInfo.Os}");

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
                    .UseSerilog()
                    .Build();

                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Serilog.Log.CloseAndFlush();
            }
        }
    }
}
