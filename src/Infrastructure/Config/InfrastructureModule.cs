using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Application.Common.Models;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Persistence;
using System.Reflection;
using System.Threading.Tasks;

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
                .AsImplementedInterfaces();

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder
                .RegisterType<PlexRipperHttpClient>()
                .As<IPlexRipperHttpClient>()
                .SingleInstance();

            builder.RegisterType<Api>().As<IApi>();

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

            Task.WaitAll(DB.Database.EnsureCreatedAsync());
        }
    }
}
