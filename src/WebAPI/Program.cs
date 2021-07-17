using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;
using Serilog;
using System;
using System.IO;
using Log = Serilog.Log;

namespace PlexRipper.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = LogConfigurationExtensions.GetLogger();

            try
            {
                Domain.Log.Information("Starting up");
                Domain.Log.Information($"Currently running on {OsInfo.CurrentOS}");

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
                Domain.Log.Fatal(e);
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }
    }
}
