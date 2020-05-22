using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Infrastructure.Persistence;

namespace PlexRipper.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<PlexRipperDbContext>();

            services.AddScoped<IPlexRipperDbContext>(provider => provider.GetService<PlexRipperDbContext>());

            // Ensure the database is created correctly
            // TODO Write this cleaner
            var _scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<PlexRipperDbContext>();

            context.Database.EnsureCreated();
            // context.Database.Migrate();

            return services;
        }
    }
}
