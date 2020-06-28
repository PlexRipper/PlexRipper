using AutoMapper;
using PlexRipper.Application.Config.Mappings;
using PlexRipper.Domain;
using PlexRipper.Domain.AutoMapper;
using PlexRipper.PlexApi.Config.Mappings;
using PlexRipper.WebAPI.Config;
using Serilog;
using System;
using Xunit.Abstractions;
using Log = Serilog.Log;

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
            var config = LogConfigurationExtensions.GetBaseConfiguration;
            return config
                .WriteTo.TestOutput(Output, outputTemplate: LogConfigurationExtensions.Template)
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
