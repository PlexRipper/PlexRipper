using AutoMapper;
using PlexRipper.Application.Config.Mappings;
using PlexRipper.Domain.AutoMapper;
using PlexRipper.PlexApi.Config.Mappings;
using PlexRipper.WebAPI.Config;
using Serilog;
using Serilog.Events;
using System;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public static class BaseDependanciesTest
    {
        static ITestOutputHelper Output;

        public static Serilog.ILogger GetLogger<T>()
        {
            return GetLoggerConfig().ForContext<T>();
        }

        public static void Setup(ITestOutputHelper output)
        {
            Environment.SetEnvironmentVariable("IntegrationTestMode", "true");
            Environment.SetEnvironmentVariable("ResetDB", "true");

            Output = output;
            SetupLogging();
        }

        public static ILogger GetLoggerConfig()
        {
            string template =
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({SourceContext:l}) {Message}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithProperty("SourceContext", null)
                // .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: template)
                .WriteTo.Debug(outputTemplate: template)
                .WriteTo.TestOutput(Output, outputTemplate: template)
                .WriteTo.ColoredConsole(LogEventLevel.Verbose, template)
                .CreateLogger();
        }

        public static void SetupLogging()
        {
            Log.Logger = GetLoggerConfig();
        }

        public static Mapper GetMapper()
        {
            var configuration = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile(new DomainMappingProfile());
                    cfg.AddProfile(new ApplicationMappingProfile());
                    cfg.AddProfile(new PlexApiMappingProfile());
                    cfg.AddProfile(new WebApiMappingProfile());
                });
            configuration.AssertConfigurationIsValid();
            return new Mapper(configuration);
        }

    }
}
