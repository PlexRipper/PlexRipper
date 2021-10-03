using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Environment;
using FluentResults;
using Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace PlexRipper.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.SetupLogging(LogEventLevel.Verbose);

            try
            {
                Log.Information("Starting up");
                Log.Information($"Currently running on {OsInfo.CurrentOS}");

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
                   // .UseSerilog()
                    .Build();

                host.Run();
            }
            catch (Exception e)
            {
               Result.Fail(new ExceptionalError(e)).LogFatal();
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }
    }
}