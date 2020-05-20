using Autofac;
using Microsoft.AspNetCore.Http;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Application.Common.Models;
using PlexRipper.Infrastructure.API.Plex;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Persistence;
using System.Linq;
using System.Reflection;
using Module = Autofac.Module;

namespace PlexRipper.Infrastructure.Config
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // register all I*Services
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            builder.RegisterType<PlexRipperHttpClient>().As<IPlexRipperHttpClient>().SingleInstance();
            builder.RegisterType<PlexApi>().As<IPlexApi>();
            builder.RegisterType<Api>().As<IApi>();

            // Database
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<PlexRipperDbContext>().As<IPlexRipperDbContext>().InstancePerLifetimeScope();

        }
    }
}
