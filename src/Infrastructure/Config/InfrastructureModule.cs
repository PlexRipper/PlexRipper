using Autofac;
using Microsoft.AspNetCore.Http;
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

            builder.RegisterType<PlexApiService>().As<IPlexApiService>().SingleInstance();

            builder.RegisterType<PlexApi>().As<IPlexApi>().InstancePerDependency();
            builder.RegisterType<Api>().As<IApi>().InstancePerDependency();
            builder.RegisterType<PlexRipperHttpClient>().As<IPlexRipperHttpClient>().SingleInstance();

            // Register Entity Framework Database
            builder.RegisterType<PlexRipperDbContext>()
                .WithParameter("options", PlexRipperDbContext.GetConfig().Options)
                .As<IPlexRipperDbContext>()
                .InstancePerLifetimeScope();

        }
    }
}
