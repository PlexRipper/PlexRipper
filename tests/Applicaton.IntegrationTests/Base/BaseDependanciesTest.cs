using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Config.Mappings;
using PlexRipper.Domain.AutoMapper;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Persistence;
using PlexRipper.WebAPI.Config;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Base
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
            Output = output;
            SetupLogging();
            var context = GetDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

        }

        public static ILogger GetLoggerConfig()
        {
            string template =
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.TestOutput(Output)
                .WriteTo.ColoredConsole(LogEventLevel.Verbose, template)
                .CreateLogger();
        }

        public static void SetupLogging()
        {
            Log.Logger = GetLoggerConfig();
        }

        public static PlexRipperDbContext GetDbContext()
        {
            // Setup DB
            var options = new DbContextOptionsBuilder<PlexRipperDbContext>()
                .UseSqlite("Data Source=PlexRipperDB_TESTS.db")
                .Options;
            return new PlexRipperDbContext(options);
        }

        public static Mapper GetMapper()
        {
            var configuration = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile(new DomainMappingProfile());
                    cfg.AddProfile(new ApplicationMappingProfile());
                    cfg.AddProfile(new InfrastructureMappingProfile());
                    cfg.AddProfile(new WebApiMappingProfile());
                });
            configuration.AssertConfigurationIsValid();
            return new Mapper(configuration);
        }

    }
}
