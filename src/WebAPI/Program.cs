using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using FluentResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;
using PlexRipper.FileSystem;
using Serilog;
using Log = Serilog.Log;

namespace PlexRipper.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // This is the temp log config until LogSystem gets called, until then nothing is logged to file.
            Log.Logger = LogSystem.GetBaseConfiguration().CreateLogger();

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