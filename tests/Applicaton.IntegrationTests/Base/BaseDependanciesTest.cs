using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Mappings;
using PlexRipper.Infrastructure.Persistence;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public static class BaseDependanciesTest
    {
        static ILogger logConfig;

        public static Serilog.ILogger GetLogger<T>()
        {
            return logConfig.ForContext<T>();
        }

        public static void Setup(ITestOutputHelper output)
        {
            SetupLogging(output);
            var context = GetDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

        }

        public static void SetupLogging(ITestOutputHelper output)
        {
            logConfig = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.TestOutput(output, LogEventLevel.Debug)
                .WriteTo.ColoredConsole(
                    LogEventLevel.Debug,
                    "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
                .CreateLogger();
            Log.Logger = logConfig;
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
                cfg => cfg.AddProfile(new ApplicationMappingProfile()));
            return new Mapper(configuration);
        }
    }
}
