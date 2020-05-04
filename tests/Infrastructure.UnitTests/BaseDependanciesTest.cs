using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Persistence;

namespace Infrastructure.UnitTests
{
    public static class BaseDependanciesTest
    {
        public static ILogger<T> GetLogger<T>()
        {
            return SetupLogging().CreateLogger<T>();
        }

        private static ILoggerFactory SetupLogging()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddDebug();
                    builder.AddConsole();
                })
                .BuildServiceProvider();

            return serviceProvider.GetService<ILoggerFactory>();
        }

        public static void Setup()
        {
            var context = GetDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
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
            var myProfile = new InfrastructureProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            return new Mapper(configuration);
        }
    }
}
