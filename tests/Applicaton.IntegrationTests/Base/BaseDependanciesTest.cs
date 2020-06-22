using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Config.Mappings;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Persistence;
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
            return new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.TestOutput(Output)
                .WriteTo.ColoredConsole(
                    LogEventLevel.Debug,
                    "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
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
                    cfg.AddProfile(new ApplicationMappingProfile());
                    cfg.AddProfile(new InfrastructureMappingProfile());
                });
            return new Mapper(configuration);
        }

    }
}
