using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.DownloadManager;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.IO;

namespace PlexRipper.WebAPI
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Log.Logger = SetupLogging();

            try
            {
                Log.Logger.Information("Starting up");

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
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<DownloadWorker>();
                    })
                    .UseSerilog()
                    .Build();

                host.Run();
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }

        public static Logger SetupLogging()
        {
            string template =
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Debug(outputTemplate: template)
                .WriteTo.ColoredConsole(LogEventLevel.Debug, template)
                .CreateLogger();
        }
    }
}
