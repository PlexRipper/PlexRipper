using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Application.Common.Models;
using PlexRipper.Infrastructure.API.Plex;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Persistence;
using PlexRipper.Infrastructure.Services;
using System.Reflection;

namespace PlexRipper.Infrastructure.Config
{
    public class InfrastructureModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // register all I*Repository
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<PlexRipperHttpClient>().As<IPlexRipperHttpClient>().SingleInstance();

            builder.RegisterType<PlexApiService>().As<IPlexApiService>();

            builder.RegisterType<PlexApi>().As<IPlexApi>().InstancePerDependency();
            builder.RegisterType<Api>().As<IApi>().InstancePerDependency();

            // Register Entity Framework Database
            builder.RegisterType<PlexRipperDbContext>()
                .WithParameter("options", PlexRipperDbContext.GetConfig().Options)
                .As<IPlexRipperDbContext>()
                .InstancePerLifetimeScope();

            builder.Register(context => context.Resolve<DbContextOptions<PlexRipperDbContext>>())
                .As<DbContextOptions>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PlexRipperDbContext>()
                .AsSelf()
                .InstancePerLifetimeScope();

            //Setup Database
            var DB = new PlexRipperDbContext(PlexRipperDbContext.GetConfig().Options);
            // TODO Re-enable Migrate when stable
            // DB.Database.Migrate();
            // DB.Database.EnsureDeleted();
            DB.Database.EnsureCreatedAsync();
        }
    }
}
